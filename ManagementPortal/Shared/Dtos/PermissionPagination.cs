namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para paginación de permisos.
    /// </summary>
    public class PermissionPagination : FilterPermissionDto
    {
        /// <summary>
        /// Lista de nombres de permisos de la página.
        /// </summary>
        public List<ApplicationPermissionDto>? Permissions { get; set; }

        /// <summary>
        /// Total de Permisos.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de paginas.
        /// </summary>
        public int TotalPages { get; set; }
    }
}