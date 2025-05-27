using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para los filtrados de roles.
    /// </summary>
    public class FilterRoleDto : FiltradosDto
    {
        /// <summary>
        /// Campo por el que se quiere ordenar.
        /// </summary>
        public SortFieldRole SortField { get; set; } = SortFieldRole.Name;

        /// <summary>
        /// Permiso que se desea buscar.
        /// </summary>
        public int SearchPermission { get; set; } = 0;
    }
}