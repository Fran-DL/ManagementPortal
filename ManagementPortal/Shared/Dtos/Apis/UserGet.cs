using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase para obtener los usuarios. (+ INFO / Detalle).
    /// </summary>
    public class UserGet : UserBasicInfo
    {
        /// <summary>
        /// Status del usuario.
        /// </summary>
        [JsonPropertyName("status")]
        required public StatusDto Status { get; set; }

        /// <summary>
        /// Imagen del usuario.
        /// </summary>
        [JsonPropertyName("picture")]
        required public string? Picture { get; set; }

        /// <summary>
        /// Firma del usuario.
        /// </summary>
        [JsonPropertyName("signature")]
        required public string? Signature { get; set; }

        /// <summary>
        /// Estado del usuario.
        /// </summary>
        [JsonPropertyName("active")]
        required public bool Active { get; set; }

        /// <summary>
        /// TenantId del usuario.
        /// </summary>
        [JsonPropertyName("tenantId")]
        required public int TenantId { get; set; }

        /// <summary>
        /// LastVersion del usuario.
        /// </summary>
        [JsonPropertyName("lastVersion")]
        required public string LastVersion { get; set; }

        /// <summary>
        /// ExternalIds del usuario.
        /// </summary>
        [JsonPropertyName("externalIds")]
        required public string ExternalIds { get; set; }
    }
}