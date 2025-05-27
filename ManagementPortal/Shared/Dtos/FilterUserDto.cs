using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para los flitrados de usuario.
    /// </summary>
    public class FilterUserDto : FiltradosDto
    {
        /// <summary>
        /// Campo por el que se quiere ordenar.
        /// </summary>
        public SortFieldUser SortField { get; set; } = SortFieldUser.Name;

        /// <summary>
        /// Se buscan usuarios eliminados.
        /// </summary>
        public UserState State { get; set; } = UserState.Both;

        /// <summary>
        /// Permiso buscado.
        /// </summary>
        public string SearchRole { get; set; } = string.Empty;
    }
}