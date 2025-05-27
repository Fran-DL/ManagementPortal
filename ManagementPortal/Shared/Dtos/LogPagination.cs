namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para paginacion de logs.
    /// </summary>
    public class LogPagination : FilterLogDto
    {
        /// <summary>
        /// Lista de logs de la página.
        /// </summary>
        public List<ApplicationAuditLogDto>? Logs { get; set; }

        /// <summary>
        /// Total de logs.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de paginas.
        /// </summary>
        public int TotalPages { get; set; }
    }
}