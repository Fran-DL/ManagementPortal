namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Entidad para modelar los mensajes recibidos por los usuarios.
    /// </summary>
    public class MessageReceiver
    {
        /// <summary>
        /// Id del mensaje recibido.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mensaje recibido.
        /// </summary>
        required public Message Message { get; set; }

        /// <summary>
        /// Usuario que crea el mensaje.
        /// </summary>
        required public ApplicationUser User { get; set; }

        /// <summary>
        /// Indica si el mensaje fue leido.
        /// </summary>
        public bool IsRead { get; set; } = false;
    }
}