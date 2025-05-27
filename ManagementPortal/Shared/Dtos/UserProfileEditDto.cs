using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// DTO usado cuando un usuario (sin permisos de edición) edita su propio perfil.
    /// Justificación: Se requiere para que los usuarios puedan editar su perfil.
    /// </summary>
    public class UserProfileEditDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileEditDto"/> class.
        /// </summary>
        public UserProfileEditDto()
        {
            UserName = string.Empty;
            Email = string.Empty;
            Name = string.Empty;
            LastName = string.Empty;
            Image = null;
        }

        /// <summary>
        /// Username del usuario (se usa para identificarlo, no se modifica).
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_username")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public string UserName { get; set; }

        /// <summary>
        /// Email del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_email")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "ApplicationUserDto_ValidEmail")]
        public string Email { get; set; }

        /// <summary>
        /// Name del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_Name")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public string Name { get; set; }

        /// <summary>
        /// LastName del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_LastName")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public string LastName { get; set; }

        /// <summary>
        /// Imagen del usuario, null si no se edita, array vacio si se elimina la imagen.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_Image")]
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public byte[]? Image { get; set; }
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly

    }
}