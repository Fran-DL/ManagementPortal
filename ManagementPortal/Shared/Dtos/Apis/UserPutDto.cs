using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Dto utilizado en los productos de sonda para modificar los datos de un usuario.
    /// </summary>
    public class UserPutDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPutDto"/> class.
        /// </summary>
        public UserPutDto()
        {
            Id = string.Empty;
            Name = string.Empty;
            LastName = string.Empty;
            UserName = string.Empty;
            Email = string.Empty;
            NewPassword = null;
            ConfirmPassword = null;
            OldPassword = null;
            Picture = string.Empty;
            Signature = string.Empty;
            Roles = new();
            RoleIds = new();
            StatusDto = new StatusDto();
            ExternalIds = string.Empty;
            LastVersion = string.Empty;
        }

        /// <summary>
        /// Id del usuario.
        /// </summary>
        [Required]
        [JsonPropertyName("id")]
        required public string Id { get; set; }

        /// <summary>
        /// Información del estado del usuario. Nota: Debería ser un DTO. NO SE MODIFICA.
        /// </summary>
        [JsonPropertyName("statusDto")]
        public StatusDto StatusDto { get; set; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        /// <summary>
        /// Correo electrónico.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// Nombre real.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Apellido.
        /// </summary>
        [JsonPropertyName("lastname")]
        public string LastName { get; set; }

        /// <summary>
        /// Nueva contraseña (opcional). NO SE MODIFICA.
        /// </summary>
        [JsonPropertyName("newPassword")]
        public string? NewPassword { get; set; }

        /// <summary>
        /// Confirmación de la nueva contraseña. NO SE MODIFICA.
        /// </summary>
        [JsonPropertyName("confirmPassword")]
        public string? ConfirmPassword { get; set; }

        /// <summary>
        /// Contraseña antigua (para confirmar cambios). NO SE MODIFICA.
        /// </summary>
        [JsonPropertyName("oldPassword")]
        public string? OldPassword { get; set; }

        /// <summary>
        /// Imagen del usuario (puede ser una URL o base64).
        /// </summary>
        [JsonPropertyName("picture")]
        public string Picture { get; set; }

        /// <summary>
        /// Firma del usuario (texto o imagen).
        /// </summary>
        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        /// <summary>
        /// Lista de roles asignados al usuario. Nota: Debería ser una lista de roles con permisos.
        /// </summary>
        [JsonPropertyName("roles")]
        public List<RoleGetDto> Roles { get; set; }

        /// <summary>
        /// Lista de roles a modificar.
        /// </summary>
        [JsonPropertyName("roleIds")]
        public List<string> RoleIds { get; set; }

        /// <summary>
        /// Ids externos.
        /// </summary>
        [JsonPropertyName("externalIds")]
        public string ExternalIds { get; set; }

        /// <summary>
        /// Last Version.
        /// </summary>
        [JsonPropertyName("lastVersion")]
        public string LastVersion { get; set; }
    }
}