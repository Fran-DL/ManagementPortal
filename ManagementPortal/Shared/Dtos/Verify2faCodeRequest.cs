using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto que transporta el codigo QR ingresado por el usuario para habilitar 2FA o loguearse si lo tiene disponible.
    /// Justificacion: Se requiere implementar 2FA para el login del usuario.
    /// </summary>
    public class Verify2faCodeRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Verify2faCodeRequest"/> class.
        /// </summary>
        public Verify2faCodeRequest()
        {
            this.Code = string.Empty;
            this.Method = string.Empty;
        }

        /// <summary>
        /// Codigo de seis digitos para 2FA.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "FieldRequired")]
        [StringLength(6, ErrorMessageResourceType = typeof(Resources.ValidationAttributes), ErrorMessageResourceName = "Verify2faCodeRequest_Code_StringLength", MinimumLength = 6)]
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "Verify2faCodeRequest_Code")]
        public string Code { get; set; }

        /// <summary>
        /// Metodo para autenticar al usuario por 2FA.
        /// </summary>
        public string Method { get; set; }
    }
}