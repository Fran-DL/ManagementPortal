using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto utilizado para transportar el valor de la nueva contraseña, su confirmación y el codigo de reseteo de contraseña.
    /// Justificacion: Se requiere para implementar el reseteo de contraseña.
    /// </summary>
    public class ResetPasswordRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResetPasswordRequest"/> class.
        /// </summary>
        public ResetPasswordRequest()
        {
            this.Username = string.Empty;
            this.NewPassword = string.Empty;
            this.ConfirmNewPassword = string.Empty;
            this.Code = string.Empty;
        }

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

        /// <summary>
        /// Codigo del usuario para cambiarle la contraseña.
        /// </summary>
        public string Code { get; set; }
    }
}