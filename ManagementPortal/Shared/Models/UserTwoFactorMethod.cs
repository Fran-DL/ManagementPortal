namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Clase que implementa la relacion User-Method.
    /// Justificacion: Se necesita asociar metodos 2FA a los usuarios.
    /// </summary>
    public class UserTwoFactorMethod
    {
        /// <summary>
        /// Id de la relacion User-Method.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id del Method en la relacion User-Method.
        /// </summary>
        public int MethodId { get; set; }

        /// <summary>
        /// Method en la relacion User-Method.
        /// </summary>
        public virtual TwoFactorMethod? Method { get; set; }

        /// <summary>
        /// Id del User en la relacion User-Method.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User en la relacion User-Method.
        /// </summary>
        public virtual ApplicationUser? User { get; set; }
    }
}