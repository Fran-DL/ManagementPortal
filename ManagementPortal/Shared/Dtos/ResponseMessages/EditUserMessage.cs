namespace ManagementPortal.Shared.Dtos.ResponseMessages
{
    /// <summary>
    /// Clase que se encarga de mostrar los mensajes de error o éxito en operaciones de asignación de productos.
    /// Justificación: Se utilizan en el endpoint de editar usuario.
    /// </summary>
    public class EditUserMessage
    {
        /// <summary>
        /// Mensajes de los productos.
        /// </summary>
        required public List<AssignProductMessage> ProductAssignMessages { get; set; }

        /// <summary>
        /// Mensaje de la edición de perfil.
        /// </summary>
        required public string UserProfileMessage { get; set; }

        /// <summary>
        /// Resultado de la operación de editar usuario.
        /// </summary>
        required public bool UserProfileResult { get; set; }
    }
}