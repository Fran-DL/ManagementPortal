namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto de canal privado o compartido para mensajería.
    /// Justificación: Representa un canal de mensajería con sus mensajes y usuarios.
    /// </summary>
    public class MessagingChannelDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingChannelDto"/> class.
        /// </summary>
        /// <param name="other">MessagingChannelDto de copia.</param>
        public MessagingChannelDto(MessagingChannelDto other)
        {
            Id = other.Id;
            Name = other.Name;
            IsPrivate = other.IsPrivate;

            Messages = new List<MessageDto>();
            foreach (var message in other.Messages)
            {
                Messages.Add(new MessageDto(message));
            }

            Users = new List<ApplicationUserDto>();
            foreach (var user in other.Users)
            {
                Users.Add(new ApplicationUserDto(user));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingChannelDto"/> class.
        /// </summary>
        public MessagingChannelDto()
        {
        }

        /// <summary>
        /// Id del canal.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del canal.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el canal es privado.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Mensajes enviados por el canal.
        /// </summary>
        public List<MessageDto> Messages { get; set; } = new();

        /// <summary>
        /// Usuarios que pertenecen al canal.
        /// </summary>
        public List<ApplicationUserDto> Users { get; set; } = new();
    }
}