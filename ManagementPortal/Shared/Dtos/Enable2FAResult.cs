namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto que implementa la respuesta (imagen de codigo QR) al habilitar 2FA por app.
    /// Justificacion: Se requiere implementar 2FA por app.
    /// </summary>
    public class Enable2FAResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Enable2FAResult"/> class.
        /// </summary>
        public Enable2FAResult()
        {
            this.QrCodeImage = string.Empty;
        }

        /// <summary>
        /// Imagen en base64 del codigo QR generado para 2FA app.
        /// </summary>
        public string QrCodeImage { get; set; }
    }
}