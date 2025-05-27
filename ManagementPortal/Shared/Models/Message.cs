using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Mensajes enviados a través de los canales de mensajería.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
            Channel = new();
            User = new();
            Receivers = new List<ApplicationUser>();
            Text = string.Empty;
            Timestamp = DateTime.UtcNow;
            IsAction = false;
        }

        /// <summary>
        /// Id del mensaje.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Canal al que pertenece el mensaje.
        /// </summary>
        public MessagingChannel Channel { get; set; }

        /// <summary>
        /// Usuario que generó el mensaje.
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Usuarios receptores del mensaje.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<ApplicationUser> Receivers { get; set; }

        /// <summary>
        /// Texto del mensaje.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Fecha de creación del mensaje.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Receptores del mensaje.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<MessageReceiver> MessageReceivers { get; set; } = new List<MessageReceiver>();

        /// <summary>
        /// Indica si un mensaje es en realidad una acción (se renderiza distinto).
        /// </summary>
        public bool IsAction { get; set; }
    }
}