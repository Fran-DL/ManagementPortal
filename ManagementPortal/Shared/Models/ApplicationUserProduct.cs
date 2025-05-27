using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Información del usuario en los otros productos de sonda.
    /// </summary>
    public class ApplicationUserProduct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserProduct"/> class.
        /// </summary>
        public ApplicationUserProduct()
        {
        }

        /// <summary>
        /// Id de la clase.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Producto del usuario.
        /// </summary>
        public Products Product { get; set; }

        /// <summary>
        /// Id del usuario en este producto.
        /// </summary>
        required public string UserProductId { get; set; }

        /// <summary>
        /// Booleano que permite controlar si la relacion usuario-producto existio pero fue borrada.
        /// Si es true, quiere decir que el usuario tuvo el producto y fue dado de baja.
        /// </summary>
        public bool ProductDelete { get; set; } = false;
    }
}