using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto que transporta las credeciales del usuario necesarias para iniciar sesion.
    /// Justificacion: Se requiere para implementar login de usuario.
    /// </summary>
    public class LoginUserRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginUserRequest"/> class.
        /// </summary>
        public LoginUserRequest()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        /// <summary>
        /// Username de la cuenta del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "LoginUserRequest_username")]
        public string Username { get; set; }

        /// <summary>
        /// Password de la cuenta del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "LoginUserRequest_password")]
        public string Password { get; set; }
    }
}