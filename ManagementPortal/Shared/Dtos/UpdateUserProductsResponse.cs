using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Resultado de actualizar los productos para un usuario.
    /// Justificacion: Se necesita para devolver los productos actualizados en Asignación de Productos.
    /// </summary>
    public class UpdateUserProductsResponse
    {
        /// <summary>
        /// Productos actualizados con exito.
        /// </summary>
        public ICollection<Products> SuccessProducts { get; set; } = new List<Products>();
    }
}