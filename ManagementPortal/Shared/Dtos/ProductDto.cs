using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Productos del usuario con sus roles.
    /// Justification: Se necesita para Alta de Usuario.
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDto"/> class.
        /// </summary>
        public ProductDto()
        {
            this.Product = Products.OthersProducts;
            this.Roles = new List<string>();
            this.ExternalIds = string.Empty;
        }

        /// <summary>
        /// Producto.
        /// </summary>
        public Products Product { get; set; }

        /// <summary>
        /// ExternalIds.
        /// </summary>
        public string ExternalIds { get; set; }

        /// <summary>
        /// Lista de id de roles del usuario para este producto.
        /// </summary>
        public List<string> Roles { get; set; }
    }
}