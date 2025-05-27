using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Clase que implementa el usuario de identity y le agrega los atributos
    /// necesarios para nuestra aplicacion.
    /// Justificacion: Se requiere el manejo de usuario con Identity.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Nombre del usuario de la aplicacion.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario de la aplicacion.
        /// </summary>
        [Required]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token para poder renovar el Jwt que maneja la aplicacion.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Atributo para modelar la key de 2FA en la app.
        /// </summary>
        public string? AuthenticatorKeyApp { get; set; }

        /// <summary>
        /// Foto de perfil del usuario.
        /// </summary>
        public byte[]? ProfilePhoto { get; set; }

        /// <summary>
        /// Expiracion del Refresh Token.
        /// </summary>
        public DateTime? ResfreshTokenExpiry { get; set; }

        /// <summary>
        /// Fecha de envío del correo para 2FA, permite manejar la expiración.
        /// </summary>
        public DateTime? TwoFactorCodeSentTime { get; set; }

        /// <summary>
        /// Indica si la instancia fue eliminada.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Ultima fecha de Login del usuario.
        /// </summary>
        public DateTime? LastLoginDate { get; set; } = null;

        /// <summary>
        /// Productos del ususario.
        /// </summary>
        public ICollection<ApplicationUserProduct>? Products { get; set; }

        /// <summary>
        /// Canales a los que pertenece el usuario.
        /// </summary>
        public virtual List<MessagingChannel> Channels { get; set; } = new List<MessagingChannel>();

        /// <summary>
        /// Mensajes enviados por el usuario.
        /// </summary>
        public virtual List<Message> SentMessages { get; set; } = new List<Message>();

        /// <summary>
        /// Mensajes recibidos por el usuario.
        /// </summary>
        public virtual ICollection<MessageReceiver> ReceivedMessages { get; set; } = new List<MessageReceiver>();
    }
}