using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Clase que implementa los Two Factor Methods utilizados en la aplicacion.
    /// Justificacion: Se requieren implementar metodos doble autentificacion.
    /// </summary>
    public class TwoFactorMethod
    {
        /// <summary>
        /// Id del metodo 2FA.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del metodo 2FA.
        /// </summary>
        public string Method { get; set; } = string.Empty;
    }
}