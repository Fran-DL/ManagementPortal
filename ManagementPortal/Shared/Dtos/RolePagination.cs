namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para paginacion de roles.
    /// </summary>
    public class RolePagination : FilterRoleDto
    {
        /// <summary>
        /// Lista de roles de la pagina.
        /// </summary>
        public List<ApplicationRoleDto>? Roles { get; set; }

        /// <summary>
        /// Total de Roles.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de paginas.
        /// </summary>
        public int TotalPages { get; set; }
    }
}