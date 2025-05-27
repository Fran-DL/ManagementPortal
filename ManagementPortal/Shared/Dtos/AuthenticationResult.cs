using System.ComponentModel.DataAnnotations;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto que transporta el token Jwt o indica que se requiere 2FA para login.
    /// Justificacion: Se require implementar Jwt entre cliente-servidor. Usuarios pueden utilizar 2FA.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Jwt que se genera para manejar la sesion del usuario.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Refresh Token que se genera para renovar el Jwt.
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Si el usuario tiene disponible un 2FA se envía el Dto con solamente este atributo.
        /// </summary>
        [Display(ResourceType = typeof(Resources.ValidationAttributes), Name = "AuthenticationResult_TwoFactorMethod")]
        public List<string> TwoFactorMethods { get; set; } = new();

        /// <summary>
        /// Booleano que se pone en true si se trata del primer login del usuario.
        /// </summary>
        public bool? FirstLogin { get; set; }
    }
}