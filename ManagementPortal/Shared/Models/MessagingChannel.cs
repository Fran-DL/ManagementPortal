using System.ComponentModel.DataAnnotations.Schema;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Canal privado o compartido para mensajería.
    /// </summary>
    public class MessagingChannel
    {
        /// <summary>
        /// Id del canal.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del canal.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el canal es privado.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Usuarios que pertenecen al canal.
        /// </summary>
        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        /// <summary>
        /// Mensajes enviados en el canal.
        /// </summary>
        public virtual List<Message> Messages { get; set; } = new List<Message>();
    }
}