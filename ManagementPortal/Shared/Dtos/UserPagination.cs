namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para paginación de usuarios.
    /// </summary>
    public class UserPagination : FilterUserDto
    {
        /// <summary>
        /// Lista de usuarios de la página.
        /// </summary>
        public List<ApplicationUserDto>? Users { get; set; }

        /// <summary>
        /// Total de usuarios.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de paginas.
        /// </summary>
        public int TotalPages { get; set; }
    }
}