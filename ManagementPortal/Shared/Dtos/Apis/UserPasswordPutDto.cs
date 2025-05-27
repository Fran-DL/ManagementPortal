using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase para modificar password de usuario.
    /// </summary>
    public class UserPasswordPutDto
    {
        /// <summary>
        /// ID.
        /// </summary>
        [Required]
        [JsonPropertyName("id")]
        required public string Id { get; set; }

        /// <summary>
        /// New password.
        /// </summary>
        [Required]
        [JsonPropertyName("newPassword")]
        required public string NewPassword { get; set; }

        /// <summary>
        /// Confirm password.
        /// </summary>
        [Required]
        [Compare(nameof(NewPassword))]
        [JsonPropertyName("confirmPassword")]
        required public string ConfirmPassword { get; set; }

        /// <summary>
        /// Old password.
        /// </summary>
        [Required]
        [JsonPropertyName("oldPassword")]
        required public string OldPassword { get; set; }
    }
}