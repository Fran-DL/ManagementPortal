using System.Security.Claims;
using ManagementPortal.Server.Services;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace ManagementPortal.Server.Controllers
{
    /// <summary>
    /// Controller encargado de agrupar funcionalidades relacionadas a las cuentas de usuarios.
    /// Justificacion: Se requieren funcionalidades de inicio de sesíon y 2FA, se agrupan en un unico controller.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;
        private readonly TwoFactorAuthService _twoFactorAuthService;
        private readonly EmailService _emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userManager">Manejador de Identity para Usuario.</param>
        /// <param name="signInManager">Manejador de Identity para Sesiones.</param>
        /// <param name="configuration">Interfaz para obtener parametros de appsettings.</param>
        /// <param name="tokenService">Servicio para manipular Jwt y Refresh Token.</param>
        /// <param name="twoFactorAuthService">Servicio para manipular funciones relacionadas con 2FA.</param>
        /// <param name="emailService">Servicio para enviar emails a traves de SMTP.</param>
        public AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    TokenService tokenService,
    TwoFactorAuthService twoFactorAuthService,
    EmailService emailService)
        {
            this._userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this._signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            this._twoFactorAuthService = twoFactorAuthService ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
            this._emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        /// <summary>
        /// Endpoint para implementar el login del usuario.
        /// </summary>
        /// <param name="request">Username y Password del Usuario.</param>
        /// <param name="product">Producto para asignar audience al token.</param>
        /// <returns>Retorna el Jwt y Refresh Token si el usuario no utiliza 2FA.
        /// Si el usuario utiliza 2FA se notifica que método tiene habilitado.
        /// </returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request, [FromHeader] Products product)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    Log.Error(LogErrorResources.UserLoginNotFound);
                    return Unauthorized(AccountResources.UserNotFound);
                }

                if (user.IsDeleted)
                {
                    Log.Error(LogErrorResources.UserIsDeleted);
                    return Unauthorized(AccountResources.UserIsInactive);
                }

                if (user.LastLoginDate == null)
                {
                    return Ok(new AuthenticationResult
                    {
                        FirstLogin = true,
                    });
                }

                await _signInManager.SignInAsync(user: user, isPersistent: false);
                var (accessToken, refreshToken) = await _tokenService.GenerateTokens(user, product);
                user.LastLoginDate = DateTime.Now; // Actualiza la ultima fecha de login del usuario luego del login exitoso a la actual.
                user.RefreshToken = refreshToken;
                var updateDate = await _userManager.UpdateAsync(user);

                if (!updateDate.Succeeded)
                {
                    return StatusCode(500, AccountResources.ErrorNewLogin);
                }

                var userInfo = new AuthenticationResult
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                };

                return Ok(userInfo);
            }

            if (result.RequiresTwoFactor)
            {
                var user = await _userManager.FindByNameAsync(request.Username);

                if (user == null)
                {
                    Log.Error(string.Format(LogErrorResources.UserNotFound, request.Username));
                    return Unauthorized(AccountResources.UserNotFound);
                }

                var twoFactorMethods = await _twoFactorAuthService.GetTwoFactorMethod(user.Id);

                if (twoFactorMethods.Count == 0)
                {
                    Log.Error(string.Format(LogErrorResources.InvalidLogin, request.Username));
                    return Unauthorized(AccountResources.InvalidLogin);
                }

                return Ok(new AuthenticationResult
                {
                    TwoFactorMethods = twoFactorMethods?
                        .Where(m => m != null)
                        .Select(m => m!.Method)
                        .ToList() ?? new List<string>(),
                });
            }

            Log.Error(string.Format(LogErrorResources.InvalidLogin, request.Username));
            return Unauthorized(AccountResources.InvalidLogin);
        }

        /// <summary>
        /// Valida la sesion del usuario dado el Jwt.
        /// Siempre se valida contra el servidor la sesion desde el cliente.
        /// </summary>
        /// <returns>Retorna 200 si el Jwt es valido o 404 sino.</returns>
        [HttpGet("Validate")]
        [Authorize]
        public async Task<IActionResult> Validate()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                return Ok();
            }

            return Unauthorized();
        }

        /// <summary>
        /// Si el usuario requiere 2FA se loguea en este endpoint.
        /// </summary>
        /// <param name="request">Codigo de seis digitos para validar el 2FA.</param>
        /// <param name="product">Producto para asignar audience al token.</param>
        /// <returns>Se retorna 404 si el usuario no tiene 2FA habilitado.
        /// Se retornan los tokens si se valida correctamente el 2FA.</returns>
        [HttpPost("LoginWith2fa")]
        public async Task<IActionResult> LoginWith2fa([FromBody] Verify2faCodeRequest request, [FromHeader] Products product)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return Unauthorized();
            }

            var twoFactorMethod = await _twoFactorAuthService.GetTwoFactorMethod(user.Id);

            if (twoFactorMethod.Count == 0)
            {
                Log.Error(string.Format(LogErrorResources.User2faNotFound, user.UserName));
                return NotFound();
            }

            Microsoft.AspNetCore.Identity.SignInResult signInResult = new();
            bool is2faTokenValid = false;

            if (request.Method == "App" && user.AuthenticatorKeyApp != null)
            {
                is2faTokenValid = _twoFactorAuthService.ValidateTotp(user.AuthenticatorKeyApp, request.Code);
            }
            else if (request.Method.Equals("Email"))
            {
                var sentTime = user.TwoFactorCodeSentTime;
                double expiryInMinutes = Convert.ToDouble(_configuration["TwoFactorEmail:ExpiryInMinutes"] ?? "30");

                var expirationTime = TimeSpan.FromMinutes(expiryInMinutes);

                if (!(DateTime.UtcNow - sentTime > expirationTime))
                {
                    signInResult = await _signInManager
                        .TwoFactorSignInAsync("Email", request.Code, isPersistent: true, rememberClient: false);
                }
            }

            if (signInResult.Succeeded || is2faTokenValid)
            {
                var (accessToken, refreshToken) = await _tokenService.GenerateTokens(user, product);
                var userInfo = new AuthenticationResult
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                };
                user.LastLoginDate = DateTime.Now;
                user.RefreshToken = refreshToken;
                var updateDate = await _userManager.UpdateAsync(user);
                return Ok(userInfo);
            }

            return Unauthorized();
        }

        /// <summary>
        /// Endpoint para solicitar el codigo QR de 2FA App.
        /// </summary>
        /// <returns>Si el Jwt no es valido retorna 404.
        /// Retorna el codigo QR asociado al usuario si el Jwt es valido.</returns>
        [HttpGet("Enable2faApp")]
        [Authorize]
        public async Task<IActionResult> Enable2faApp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(AccountResources.UserNotFound);
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

            if (authenticatorKey == null || string.IsNullOrWhiteSpace(authenticatorKey))
            {
                Log.Error(LogErrorResources.ErrorKey);
                return NotFound(AccountResources.ErrorKey);
            }

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                Log.Error(LogErrorResources.ErrorKey);
                return NotFound(AccountResources.ErrorKey);
            }

            var url = _twoFactorAuthService.GenerateQrCodeUri(authenticatorKey, user.Email, "Management Portal");
            var qrCodeImage = _twoFactorAuthService.GenerateQrCode(url);

            await _twoFactorAuthService.RemoveTwoFactorMethod(user.Id, "app");
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            return Ok(new Enable2FAResult
            {
                QrCodeImage = qrCodeImage,
            });
        }

        /// <summary>
        /// Endpoint para verificar el codigo de seis digitos cuando Metodo = App.
        /// Se habilita el metodo si el codigo es correcto.
        /// </summary>
        /// <param name="verify2FaCodeRequest">Codigo de seis digitos que ingresa el usuario.</param>
        /// <returns>Retonar 400 si el Jwt no es valido o el codigo es incorrecto.
        /// Retorna 200 si el codigo se valida correctamente.
        /// </returns>
        [HttpPost("Verify2faApp")]
        [Authorize]
        public async Task<IActionResult> Verify2faApp([FromBody] Verify2faCodeRequest verify2FaCodeRequest)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound();
            }

            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

            if (authenticatorKey == null)
            {
                return BadRequest();
            }

            var is2faTokenValid = _twoFactorAuthService.ValidateTotp(authenticatorKey, verify2FaCodeRequest.Code);

            if (!is2faTokenValid)
            {
                return BadRequest();
            }

            await _twoFactorAuthService.SetTwoFactorMethod(user.Id, "App");
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            user.AuthenticatorKeyApp = authenticatorKey;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        /// <summary>
        /// Metodo para validar 2FA en recuperación de contraseña.
        /// </summary>
        /// <param name="verify2FaCodeRequest">Modelo para enviar codigo.</param>
        /// <param name="username">Username del usaurio.</param>
        /// <param name="resetToken">Token de reseteo de password del usaurio.</param>
        /// <returns>Retorna OK si se pudo validar correctamente el código.</returns>
        [HttpPost("Verify2faResetPassword/{username}/{resetToken}")]
        public async Task<IActionResult> Verify2faAppResetPassword(
            [FromBody] Verify2faCodeRequest verify2FaCodeRequest, string username, string resetToken)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return NotFound();
            }

            string token = Uri.UnescapeDataString(resetToken);
            var isValidToken = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", token);

            if (!isValidToken)
            {
                Log.Error(string.Format(LogErrorResources.InvalidToken, username));
                return Unauthorized(AccountResources.InvalidToken);
            }

            var is2faTokenValid = false;
            var tokenoption = TokenOptions.DefaultAuthenticatorProvider;
            if (verify2FaCodeRequest.Method.Equals("App"))
            {
                is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultAuthenticatorProvider,
                verify2FaCodeRequest.Code);
            }
            else if (verify2FaCodeRequest.Method.Equals("Email"))
            {
                var sentTime = user.TwoFactorCodeSentTime;
                var expirationTime = TimeSpan.FromMinutes(1);

                if (DateTime.UtcNow - sentTime > expirationTime)
                {
                    return NotFound();
                }

                is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                verify2FaCodeRequest.Code);
            }

            if (!is2faTokenValid)
            {
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// Endpoint para enviar el codigo de seis digitos de 2FA Email.
        /// </summary>
        /// <returns>Si el Jwt no es valido retorna 404.
        /// Retorna codigo 200 si el Jwt es valido y se pudo enviar el mail.
        /// </returns>
        [HttpGet("Enable2faEmail")]
        [Authorize]
        public async Task<IActionResult> Enable2faEmail()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound(UsersResources.UserNotFound);
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            var authenticatorkey = await _userManager.GetAuthenticatorKeyAsync(user);

            if (authenticatorkey == null)
            {
                return BadRequest();
            }

            await _twoFactorAuthService.RemoveTwoFactorMethod(user.Id, "email");
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            await _twoFactorAuthService.SendEmailTwoFactorCode(user);

            return Ok();
        }

        /// <summary>
        /// Endpoint para verificar el codigo de seis digitos cuando Metodo = Email.
        /// Se habilita el metodo si el codigo es correcto.
        /// </summary>
        /// <param name="verify2FaCodeRequest">Codigo de seis digitos que ingresa el usuario.</param>
        /// <returns>Retonar 400 si el Jwt no es valido o el codigo es incorrecto.
        /// Retorna 200 si el codigo se valida correctamente.
        /// </returns>
        [HttpPost("Verify2faEmail")]
        [Authorize]
        public async Task<IActionResult> Verify2faEmail([FromBody] Verify2faCodeRequest verify2FaCodeRequest)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound();
            }

            var sentTime = user.TwoFactorCodeSentTime;
            double expiryInMinutes = Convert.ToDouble(_configuration["TwoFactorEmail:ExpiryInMinutes"] ?? "30");

            var expirationTime = TimeSpan.FromMinutes(expiryInMinutes);

            if (DateTime.UtcNow - sentTime > expirationTime)
            {
                return NotFound();
            }

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                verify2FaCodeRequest.Code);

            if (!is2faTokenValid)
            {
                return BadRequest();
            }

            var userMethods = await _twoFactorAuthService.GetTwoFactorMethod(user.Id);

            if (userMethods != null && !userMethods.Any(p => p?.Method != null && p.Method.ToLower().Equals("email")))
            {
                await _twoFactorAuthService.SetTwoFactorMethod(user.Id, "Email");
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            }

            return Ok();
        }

        /// <summary>
        /// Metodo para eliminar un metodo de 2FA asociado a un usuario.
        /// </summary>
        /// <param name="method">Nombre del metodo a borrar.</param>
        /// <param name="username">Username del usuario a borrarle el metodo.</param>
        /// <returns>Retorna 200 si se pudo realizar la operación. De lo contrario 400.</returns>
        [HttpDelete("{username}/Delete2faMethod")]
        [Authorize]
        public async Task<IActionResult> Delete2faMethod([FromQuery] string method, [FromRoute] string username)
        {
            var userPermission = User.Claims
                          .Where(c => c.Type == ClaimTypes.Role)
                          .Select(c => c.Value)
                          .ToList();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return NotFound();
            }

            if (user.UserName != username && !userPermission.Contains(ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Update2fa]))
            {
                Log.Error(string.Format(LogErrorResources.InvalidRole, ManagementPortalPermission.PermissionDictionary[ManagementPortalPermission.Permission.Update2fa]));
                return Unauthorized();
            }

            var userMethod = await _userManager.FindByNameAsync(username);

            if (userMethod == null)
            {
                return BadRequest(UsersResources.UserNotFound);
            }
            else
            {
                try
                {
                    await _twoFactorAuthService.RemoveTwoFactorMethod(userMethod.Id, method);
                    var userMethods = await _twoFactorAuthService.GetTwoFactorMethod(user.Id);
                    await _userManager.SetTwoFactorEnabledAsync(userMethod, userMethods.Count() > 0);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return BadRequest(ex.Message);
                }

                return Ok();
            }
        }

        /// <summary>
        /// Endpoint para enviar el mail de 2FA.
        /// </summary>
        /// <returns>Retonar OK si se pudo enviar el email al usuario y el codigo de cambio de password es valido.</returns>
        [HttpPost("SendCodeEmail")]
        public async Task<IActionResult> SendCodeEmail()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                Log.Error(LogErrorResources.UserLoginNotFound);
                return Unauthorized();
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _twoFactorAuthService.SendEmailTwoFactorCode(user);

            return Ok();
        }

        /// <summary>
        /// Endpoint para enviar el mail de 2FA para el reseteo de password.
        /// </summary>
        /// <param name="username">username al que le llegara el mail.</param>
        /// /// <param name="codigo">codigo que se usara para el cambio de password.</param>
        /// <returns>Retonar OK si se pudo enviar el email.</returns>
        [HttpPost("SendCodeEmail/{username}/{codigo}")]
        public async Task<IActionResult> SendCodeEmail(string username, string codigo)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return NotFound();
            }

            string token = Uri.UnescapeDataString(codigo);
            var isValidToken = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", token);

            if (!isValidToken)
            {
                Log.Error(string.Format(LogErrorResources.InvalidToken, username));
                return Unauthorized(AccountResources.InvalidToken);
            }

            await _twoFactorAuthService.SendEmailTwoFactorCode(user);

            return Ok();
        }

        /// <summary>
        /// Endpoint para enviar mail para recuperar contraseña.
        /// </summary>
        /// <param name="username">Username del usuario.</param>
        /// <returns>Retonar OK si se pudo enviar el email.</returns>
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return NotFound();
            }

            if (user.UserName == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var applicationUrl = _configuration["ApplicationUrl"];

            var callbackUrl = AccountResources.ResetPasswdMessage + "\n" +
                $"{applicationUrl}resetPassword/?codigo={Uri.EscapeDataString(token)}&username={Uri.EscapeDataString(user.UserName)}";

            if (user.Email != null && callbackUrl != null)
            {
                await _emailService.SendEmailAsync(user.Email, "Reset Password", callbackUrl);
            }
            else
            {
                Log.Error(string.Format(LogErrorResources.RecoverPasswordError, username));
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// Endpoint para validar el código de recuperación.
        /// </summary>
        /// <param name="username">Username del usuario.</param>
        /// <param name="codigo">Código de recuperación.</param>
        /// <returns>Retorna OK si el código es válido. Indica si el usuario tiene 2FA habilitado o si la validación falla.</returns>
        [HttpPost("ValidateResetPasswordToken")]
        public async Task<IActionResult> ValidateResetPasswordToken(string username, string codigo)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return Unauthorized(AccountResources.UserNotFound);
            }

            string token = Uri.UnescapeDataString(codigo);
            var isValidToken = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "ResetPassword", token);

            if (!isValidToken)
            {
                Log.Error(string.Format(LogErrorResources.InvalidToken, username));
                return Unauthorized(AccountResources.InvalidToken);
            }

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                var twoFactorMethods = await _twoFactorAuthService.GetTwoFactorMethod(user.Id);
                return Ok(new AuthenticationResult
                {
                    TwoFactorMethods = twoFactorMethods?
                        .Where(m => m != null)
                        .Select(m => m!.Method)
                        .ToList() ?? new List<string>(),
                });
            }

            return Ok(new AuthenticationResult { });
        }

        /// <summary>
        /// Endpoint para resetear la contraseña de un usuario.
        /// </summary>
        /// <param name='resetPasswordRequest'>Se pasan username, NewPassword, ConfirmNewPassword, Code para el cambio de contraseña.</param>
        /// <returns>Retorna OK si se cambia la contraseña.</returns>
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            string username = resetPasswordRequest.Username;
            string codigo = resetPasswordRequest.Code;
            string newPassword = resetPasswordRequest.NewPassword;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                Log.Error(string.Format(LogErrorResources.UserNotFound, username));
                return Unauthorized(AccountResources.UserNotFound);
            }

            string token = Uri.UnescapeDataString(codigo);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        /// Endpoint para refrescar el token.
        /// </summary>
        /// <param name="refreshToken">Refresh token.</param>
        /// <param name="product">Producto para asignar audience al token.</param>
        /// <returns>Retorna respuesta nuevo token en la respuesta.</returns>
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken, [FromHeader] Products product)
        {
            try
            {
                var tokens = await _tokenService.RefreshTokens(refreshToken, product);
                return Ok(new AuthenticationResult
                {
                    Token = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken,
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized();
            }
        }
    }
}