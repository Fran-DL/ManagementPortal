using System.ComponentModel.DataAnnotations;
using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// DTO usado para editar los campos o roles de un usuario en un producto.
    /// Justificación: se necesita para poder editar los campos de un usuario en un producto.
    /// </summary>
    public class UserProductEditDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserProductEditDto"/> class.
        /// </summary>
        public UserProductEditDto()
        {
            Product = Products.ManagementPortal;
            ExternalIds = null;
            RolesId = new List<string>();
        }

        /// <summary>
        /// Producto del usuario.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "UserProduct_product")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public Products Product { get; set; }

        /// <summary>
        /// Identificador del usuario en otros sistemas de terceros.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "UserProduct_externalIds")]
        public string? ExternalIds { get; set; }

        /// <summary>
        /// Lista de id de los roles del usuario en el producto.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "UserCompleteEdit_RoleID")]
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        public List<string> RolesId { get; set; }
    }
}