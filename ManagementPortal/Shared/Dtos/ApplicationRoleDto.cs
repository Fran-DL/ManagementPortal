using System.ComponentModel.DataAnnotations;
using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto de rol.
    /// Justificacion: Se necesita un dto para transportar información de roles.
    /// </summary>
    public class ApplicationRoleDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRoleDto"/> class.
        /// </summary>
        public ApplicationRoleDto()
        {
            this.Id = string.Empty;
            this.Name = string.Empty;
            this.Permissions = new List<ApplicationPermissionDto>();
            this.ApplicationManagment = Products.ManagementPortal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRoleDto"/> class.
        /// Crea un rol dto a partir de otro.
        /// </summary>
        /// <param name="other">DTO para crear la copia.</param>
        public ApplicationRoleDto(ApplicationRoleDto other)
        {
            this.Id = other.Id;
            this.Name = other.Name;
            this.Permissions = new List<ApplicationPermissionDto>(other.Permissions ?? new List<ApplicationPermissionDto>());
            this.ApplicationManagment = other.ApplicationManagment;
        }

        /// <summary>
        /// Id del rol.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationRole_Name")]
        public string Name { get; set; }

        /// <summary>
        /// Permisos que pertenecen al rol.
        /// </summary>
        public ICollection<ApplicationPermissionDto>? Permissions { get; set; }

        /// <summary>
        /// Producto al que pertence el rol.
        /// </summary>
        public Products ApplicationManagment { get; set; }
    }
}