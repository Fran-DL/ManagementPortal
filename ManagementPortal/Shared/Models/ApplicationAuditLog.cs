namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Información respecto a las acciónes realizadas para la auditoria.
    /// </summary>
    public class ApplicationAuditLog
    {
        /// <summary>
        /// Id del log.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Fecha y hora del momento cuando se realiza el log.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Id de la acción que se esta realizando.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Id del usuario que realiza la acción.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Nivel del log.
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// IP de origen.
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Nombre del producto de origen.
        /// </summary>
        public string? Application { get; set; }

        /// <summary>
        /// Body de la request.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}