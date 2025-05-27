using ManagementPortal.Server.Models;
using OtpNet;

namespace ManagementPortal.Server.Services
{
    /// <summary>
    /// Clase que simula un servicio de OTP, para generar y validar códigos OTP de los dummies.
    /// </summary>
    public class OtpDummyService
    {
        private static Dictionary<string, OtpInfoDTO> _otpStore = new();
        private readonly TimeSpan _otpExpiration = TimeSpan.FromMinutes(5); // Tiempo de vida del OTP

        /// <summary>
        /// Genera un OTP para un usuario y un producto.
        /// </summary>
        /// <param name="userId">Usuario que quiere logearse en producto.</param>
        /// <param name="product"> Producto elegido para logearse.</param>
        /// <returns>Devuelve el codigo OTP.</returns>
        public string GenerateOtp(string userId, string product)
        {
            // Código secreto único para el usuario
            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);

            // Creo una instancia de Totp con la clave generada
            var otp = new Totp(secretKey);

            // Genero el código OTP basado en el tiempo actual
            string otpCode = otp.ComputeTotp();

            var otpInfo = new OtpInfoDTO
            {
                UserId = userId,
                Product = product,
                ExpiresAt = DateTime.UtcNow.Add(_otpExpiration),
            };

            _otpStore[otpCode] = otpInfo;

            return otpCode;
        }

        /// <summary>
        /// Verifica si el OTP es valido, buscandolo en memoria y validando que no haya expirado.
        /// </summary>
        /// <param name="otp"> Codigo Otp pasado por MP.</param>
        /// <param name="userId"> userId del usuario.</param>
        /// <param name="product"> Producto elegido para logearse.</param>"
        /// <returns>Retorna true solo si es un OTP valido.</returns>
        public bool ValidateOtp(string otp, string userId, string product)
        {
            // si no esta en el diccionario, no es valido.
            if (!_otpStore.TryGetValue(otp, out var otpInfo))
            {
                return false;
            }

            // Si se agoto el tiempo, no es valido.
            if (otpInfo.ExpiresAt < DateTime.UtcNow || !string.Equals(otpInfo.UserId, userId))
            {
                return false;
            }

            // Si el producto no es el mismo, no es valido.
            if (!string.Equals(otpInfo.Product, product))
            {
                return false;
            }

            return true; // Si no se cumple ninguna de las condiciones anteriores, es valido.
        }
    }
}