namespace ManagementPortal.Server.Models
{
    /// <summary>
    /// Clase que representa la información de un OTP.
    /// </summary>
    public class OtpInfoDTO
    {
        /// <summary>
        /// Usuario al que pertenece el OTP.
        /// </summary>
        required public string UserId { get; set; }

        /// <summary>
        /// Producto al que pertenece el OTP.
        /// </summary>
        required public string Product { get; set; }

        /// <summary>
        /// Fecha de expiración del OTP.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}