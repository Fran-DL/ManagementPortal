using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para los firltados de permisos.
    /// </summary>
    public class FilterPermissionDto : FiltradosDto
    {
        /// <summary>
        /// Campo por el que se quiere ordenar.
        /// </summary>
        public SortFieldPermission SortField { get; set; } = SortFieldPermission.Name;
    }
}