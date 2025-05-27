using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto utilizado para transportar el valor de la contraseña antigua, la nueva y su confirmación.
    /// Justificacion: Se requiere para implementar el cambio de contraseña.
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordRequest"/> class.
        /// </summary>
        public ChangePasswordRequest()
        {
            this.Username = string.Empty;
            this.CurrentPassword = string.Empty;
            this.NewPassword = string.Empty;
            this.ConfirmNewPassword = string.Empty;
        }

        /// <summary>
        /// La contraseña anterior del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ChangePasswordRequest_currentPassword")]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// La nueva contraseña del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ChangePasswordRequest_newPassword")]
        public string NewPassword { get; set; }

        /// <summary>
        /// La contraseña anterior del usuario.
        /// </summary>
        [Compare("NewPassword", ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "ChangePasswordRequest_compareNewPassword")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ChangePasswordRequest_confirmNewPassword")]
        public string ConfirmNewPassword { get; set; }

        /// <summary>
        /// Username del usuario a cambiarle la contraseña.
        /// </summary>
        public string Username { get; set; }
    }
}