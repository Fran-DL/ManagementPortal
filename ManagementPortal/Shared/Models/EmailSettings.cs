namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Clase que permite tomar los parametros del appsettings para SMTP.
    /// Justificacion: Demasiados parametros como para tomarlos por separados, decidimos agruparlos.
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Servidor SMPT desde el que se envia el mail.
        /// </summary>
        public string SmtpServer { get; set; } = string.Empty;

        /// <summary>
        /// Puerto del servidor STMP.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Email desde el que se envia el mail.
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// Nombre desde el que se envía el mail.
        /// </summary>
        public string SenderName { get; set; } = string.Empty;

        /// <summary>
        /// Username de credenciales para STMP.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password de credenciales para SMPT.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}