using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase que se utiliza para almacenar la info de los roles.
    /// </summary>
    public class RoleDummy
    {
        /// <summary>
        /// ID del rol.
        /// </summary>
        required public string Id { get; set; }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        required public string Name { get; set; }

        /// <summary>
        /// Producto al que pertenece el rol.
        /// </summary>
        required public Products Producto { get; set; }

        /// <summary>
        /// Lista de permisos del rol.
        /// </summary>
        required public List<PermissionDummy> Permissions { get; set; }
    }
}