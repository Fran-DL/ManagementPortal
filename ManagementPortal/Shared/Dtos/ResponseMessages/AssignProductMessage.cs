namespace ManagementPortal.Shared.Dtos.ResponseMessages
{
    /// <summary>
    /// Clase que se encarga de mostrar los mensajes de error o éxito en operaciones de asignación de productos.
    /// Se utilizan en el endpoint de editar usuario.
    /// </summary>
    public class AssignProductMessage
    {
        /// <summary>
        /// Producto con el cual se intentó realizar la operación.
        /// </summary>
        required public string Product { get; set; }

        /// <summary>
        /// Mensaje de error o exito de la operación.
        /// </summary>
        required public string Message { get; set; }

        /// <summary>
        /// Resultado de la operación.
        /// </summary>
        required public bool Result { get; set; }
    }
}