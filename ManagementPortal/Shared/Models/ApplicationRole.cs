using Microsoft.AspNetCore.Identity;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Clase que implementa los roles. Extiende IdentityRole para aprovecharse de los manejadores.
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        /// <summary>
        /// Permisos que pertenecen al rol.
        /// </summary>
        public ICollection<ApplicationPermission>? Permissions { get; set; }
    }
}