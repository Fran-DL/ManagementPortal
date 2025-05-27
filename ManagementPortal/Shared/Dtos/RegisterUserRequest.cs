using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para transportar los datos del usuario a registrar.
    /// Justifacion: Necesario para implementar Alta de usuario.
    /// </summary>
    public class RegisterUserRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterUserRequest"/> class.
        /// </summary>
        public RegisterUserRequest()
        {
            this.Name = string.Empty;
            this.LastName = string.Empty;
            this.UserName = string.Empty;
            this.Email = string.Empty;
            this.Password = string.Empty;
            this.ConfirmPassword = string.Empty;
            this.TwoFactorMethod = null;
            this.MPRoles = new List<string>();
            this.Products = new List<ProductDto>();
            this.Image = null;
        }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes),     ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_Name")]
        public string Name { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Username del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]

        // [StringLength(15, ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "RegisterUserRequest_UsernameLength")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_Username")]
        public string UserName { get; set; }

        /// <summary>
        /// Email del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_Email")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "RegisterUserRequest_ValidEmail")]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_Password")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "RegisterUserRequest_Password_StringLength", MinimumLength = 6)]
        [RegularExpression("^(?=.*[A-Z])(?=.*\\d)(?=.*[^\\w\\s]).+$", ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "RegisterUserRequest_PasswordValidation")]
        [Compare(nameof(ConfirmPassword), ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "RegisterUserRequest_PasswordDoNotMatch")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmar la contraseña.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_ConfirmPassword")]
        [Compare(nameof(Password), ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "RegisterUserRequest_PasswordDoNotMatch")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Nombre del metodo 2FA del usuario (si lo tiene).
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "RegisterUserRequest_TwoFactorMethod")]
        public string? TwoFactorMethod { get; set; }

        /// <summary>
        /// Lista de roles del MP asignados al usuario (si los tiene).
        /// </summary>
        public List<string> MPRoles { get; set; }

        /// <summary>
        /// Productos del usuario y su información correspondiente (ver ProductDto).
        /// </summary>
        public List<ProductDto> Products { get; set; }

        /// <summary>
        /// Imagen de perfil del usuario.
        /// </summary>
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public byte[]? Image { get; set; }
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
    }
}