using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Utilizado en el ALTA de USUARIO.
    /// </summary>
    public class UserPostDto
    {
        /// <summary>
        /// UserName.
        /// </summary>
        [Required]
        [JsonPropertyName("userName")]
        required public string UserName { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        required public string Email { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        [Required]
        [JsonPropertyName("name")]
        required public string Name { get; set; }

        /// <summary>
        /// LastName.
        /// </summary>
        [Required]
        [JsonPropertyName("lastName")]
        required public string LastName { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        [Required]
        [JsonPropertyName("password")]
        required public string Password { get; set; }

        /// <summary>
        /// ConfirmPassword.
        /// </summary>
        [Required]
        [Compare(nameof(Password))]
        [JsonPropertyName("confirmPassword")]
        required public string ConfirmPassword { get; set; }

        /// <summary>
        /// Roles.
        /// </summary>
        [JsonPropertyName("roleIds")]
        public List<string>? RoleIds { get; set; }

        /// <summary>
        /// Imagen.
        /// </summary>
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        /// <summary>
        /// Firma.
        /// </summary>
        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        /// <summary>
        /// External Id's.
        /// </summary>
        [JsonPropertyName("externalIds")]
        public string? ExternalIds { get; set; } // Añadido No está en la API.

        /// <summary>
        /// Last Version.
        /// </summary>
        [JsonPropertyName("lastVersion")]
        public string? LastVersion { get; set; } // Añadido No está en la API.
    }
}