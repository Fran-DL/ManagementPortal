using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using QRCoder;

namespace ManagementPortal.Server.Services
{
    /// <summary>
    /// Servicio que implementa las funcionalidad para 2FA.
    /// Justificacion: Se requeire implementar 2FA en la aplicacion.
    /// Se agrupan funcionalidades en este servicio pues se consumen desde varios endpoints.
    /// </summary>
    public class TwoFactorAuthService
    {
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorAuthService"/> class.
        /// </summary>
        /// <param name="emailService">Servicio que implementa funcionalidades SMTP.</param>
        /// <param name="userManager">Manejador de Identity para Usuarios.</param>
        /// <param name="dbContext">Context para poder persistir metodos.</param>
        public TwoFactorAuthService(EmailService emailService, UserManager<ApplicationUser> userManager, ApplicationContext dbContext)
        {
            this._userManager = userManager;
            this._emailService = emailService;
            this._dbContext = dbContext;
        }

        /// <summary>
        /// Metodo para generar la url del codigo QR.
        /// </summary>
        /// <param name="secretKey">Llave para el codigo QR.</param>
        /// <param name="userEmail">Email del usuario a generar el codigo QR.</param>
        /// <param name="issuer">Nombre de la aplicacion que genera el QR.</param>
        /// <returns>Retorna una url que deberá ser parseada a un codigo QR.</returns>
        public string GenerateQrCodeUri(string secretKey, string userEmail, string issuer)
        {
            var uri = $"otpauth://totp/{issuer}:{userEmail}?secret={secretKey}&issuer={issuer}";
            return uri;
        }

        /// <summary>
        /// Metodo que genera un codigo QR a partir de una url.
        /// </summary>
        /// <param name="uri">Url desde la cual se genera el codigo QR.</param>
        /// <returns>Retorna el codigo QR que deberá ser utilizado en una authentication app.</returns>
        public string GenerateQrCode(string uri)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        /// <summary>
        /// Metodo que genera el codigo de verificacion para 2FA en Email.
        /// </summary>
        /// <param name="applicationUser">Usuario al que se deberá enviarle un email para 2FA.</param>
        /// <returns>No regresa nada ya que se implementa como un Task.</returns>
        public async Task SendEmailTwoFactorCode(ApplicationUser applicationUser)
        {
            var code = await _userManager.GenerateTwoFactorTokenAsync(applicationUser, TokenOptions.DefaultEmailProvider);

            applicationUser.TwoFactorCodeSentTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(applicationUser);

            if (code == null || string.IsNullOrEmpty(applicationUser.Email))
            {
                return;
            }

            await _emailService.SendEmailAsync(applicationUser.Email, ServicesResources.VerificationCode, code.ToString());
        }

        /// <summary>
        /// Metodo para validar Totp.
        /// </summary>
        /// <param name="secretKey">Secret key del totp.</param>
        /// <param name="userCode">Codigo ingresado por el usuario.</param>
        /// <returns>Retorna ok si se valida correctamente.</returns>
        public bool ValidateTotp(string secretKey, string userCode)
        {
            // Convierte la clave secreta desde Base32, si es necesario
            var otpSecret = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(otpSecret);

            var generatedCode = totp.ComputeTotp();

            return totp.VerifyTotp(userCode, out _, new VerificationWindow(previous: 0, future: 0));
        }

        /// <summary>
        /// Metodo que setea el metodo de autentificacion para un usuario dado.
        /// </summary>
        /// <param name="idUser">Id de Identity asignado al usuario.</param>
        /// <param name="twoFactorMethod">Metodo de 2FA, puede ser App o Email.</param>
        /// <returns>No regresa nada ya que se implementa como un Task.</returns>
        public async Task SetTwoFactorMethod(string idUser, string twoFactorMethod)
        {
            var method = this._dbContext.TwoFactorMethods
            .FirstOrDefault(tm => tm.Method == twoFactorMethod);

            if (method != null)
            {
                var userTwoFactor = new UserTwoFactorMethod
                {
                    UserId = idUser,
                    MethodId = method.Id,
                };

                this._dbContext.UserTwoFactorMethods.Add(userTwoFactor);
                await this._dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Elimina un metodo de 2FA asociado a un usuario.
        /// </summary>
        /// <param name="idUser">Id de Identity del usuario.</param>
        /// <param name="twoFactorMethod">Nombre del metodo.</param>
        /// <returns>No regresa nada ya que se implementa como un Task.</returns>
        public async Task RemoveTwoFactorMethod(string idUser, string twoFactorMethod)
        {
            var userMethods = await _dbContext.UserTwoFactorMethods
                .Include(tm => tm.Method)
                .Where(um => um.UserId == idUser)
                .ToListAsync();

            var method = userMethods.FirstOrDefault(
                um => um.Method != null &&
                um.Method.Method.ToLower().Equals(twoFactorMethod.ToLower()));

            if (method != null)
            {
                _dbContext.UserTwoFactorMethods.Remove(method);
                await this._dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Metodo que obtiene el metodo de 2FA asociado a un usuario dado.
        /// </summary>
        /// <param name="idUser">Id de Identity asignado al usuario.</param>
        /// <returns>Retonar el metodo de 2FA que tiene habilitado el usuario.</returns>
        public async Task<List<TwoFactorMethod?>> GetTwoFactorMethod(string idUser)
        {
            var res = await this._dbContext.UserTwoFactorMethods.ToListAsync();
            var userMethods = await this._dbContext.UserTwoFactorMethods
                .Include(tm => tm.Method)
                .Where(um => um.UserId == idUser)
                .ToListAsync();

            var resMethods = userMethods.Select(um => um.Method).ToList();

            return resMethods;
        }
    }
}