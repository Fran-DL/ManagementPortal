using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para trasnportar datos del usuario.
    /// Justifacion: Solamente queremos exponer algunos datos del usuario.
    /// </summary>
    public class ApplicationUserDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserDto"/> class.
        /// </summary>
        public ApplicationUserDto()
        {
            this.Id = string.Empty;
            this.Name = string.Empty;
            this.LastName = string.Empty;
            this.UserName = string.Empty;
            this.Email = string.Empty;
            this.TwoFactorMethod = null;
            this.IsDeleted = false;
            this.Image = null;
            this.LastLoginDate = null;
            this.Roles = new List<ApplicationRoleDto>();
            this.Products = new List<ApplicationUserProductDto>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserDto"/> class.
        /// </summary>
        /// <param name="original">DTO original a copiar.</param>
        /// <exception cref="ArgumentNullException">Excepcion si el DTO original es null.</exception>
        public ApplicationUserDto(ApplicationUserDto original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            Id = original.Id;
            Name = original.Name;
            LastName = original.LastName;
            UserName = original.UserName;
            Email = original.Email;
            TwoFactorMethod = original.TwoFactorMethod;
            IsDeleted = original.IsDeleted;
            Image = original.Image;
            LastLoginDate = original.LastLoginDate;
            Roles = new List<ApplicationRoleDto>(original.Roles);
            Products = new List<ApplicationUserProductDto>(original.Products);
        }

        /// <summary>
        /// Id del usuario.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_Name")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public string Name { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_LastName")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public string LastName { get; set; }

        /// <summary>
        /// Username del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_username")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_email")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "ApplicationUserDto_ValidEmail")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de los métodos 2FA del usuario (si tiene).
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "ApplicationUser_twoFactorMethod")]
        public List<string>? TwoFactorMethod { get; set; }

        /// <summary>
        /// True si el usuario fue eliminado.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Ultima fecha de Login del usuario.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Imagen de perfil del usuario.
        /// </summary>
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public byte[]? Image { get; set; }
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly

        /// <summary>
        /// Lista de roles del usuario.
        /// </summary>
        public List<ApplicationRoleDto> Roles { get; set; }

        /// <summary>
        /// Productos asociados al usuario con su información correspondiente.
        /// </summary>
        public List<ApplicationUserProductDto> Products { get; set; }
    }
}