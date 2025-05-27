using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Mensajes enviados a través de los canales de mensajería.
    /// Justificación: Se necesita para transportar los mensajes enviados a través de los canales de mensajería.
    /// </summary>
    public class MessageDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDto"/> class.
        /// </summary>
        public MessageDto()
        {
            Id = Guid.NewGuid();
            User = new();
            Text = string.Empty;
            Timestamp = DateTime.UtcNow;
            MessagingChannel = new();
            IsAction = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDto"/> class.
        /// </summary>
        /// <param name="other">MessageDTO de copia.</param>
        public MessageDto(MessageDto other)
        {
            Id = other.Id;
            User = new ApplicationUserDto(other.User);
            Text = other.Text;
            Timestamp = other.Timestamp;
            MessagingChannel = new MessagingChannelDto(other.MessagingChannel);
            State = other.State;
            IsAction = other.IsAction;
        }

        /// <summary>
        /// Id del mensaje.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Usuario que generó el mensaje.
        /// </summary>
        public ApplicationUserDto User { get; set; }

        /// <summary>
        /// Texto del mensaje.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Fecha de creación del mensaje.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Indica si el mensaje fue leído.
        /// </summary>
        public MessageState State { get; set; } = MessageState.Send;

        /// <summary>
        /// Canal al que pertenece el mensaje.
        /// </summary>
        public MessagingChannelDto MessagingChannel { get; set; }

        /// <summary>
        /// Indica si un mensaje es en realidad una acción (se renderiza distinto).
        /// </summary>
        public bool IsAction { get; set; }
    }
}