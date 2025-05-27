using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// DTO para editar campos que requieren permiso de edición (roles y productos).
    /// Justificación: Se requiere para implementar la modificación de usuario.
    /// </summary>
    public class UserCompleteEditDto : UserProfileEditDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCompleteEditDto"/> class.
        /// </summary>
        public UserCompleteEditDto()
            : base()
        {
            Products = new List<UserProductEditDto>();
            RolesMP = new List<string>();
        }

        /// <summary>
        /// Productos asociados al usuario con su información correspondiente.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "FieldRequired")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public List<UserProductEditDto> Products { get; set; }

        /// <summary>
        /// Lista de roles del usuario en el mp.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "UserCompleteEdit_RoleID")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public List<string> RolesMP { get; set; }
    }
}