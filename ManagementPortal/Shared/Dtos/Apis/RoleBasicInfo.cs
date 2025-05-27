using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase básica con la información de los roles. Se utiliza en el Get del listado de usuarios. y El listado de roles.
    /// </summary>
    public class RoleBasicInfo
    {
        /// <summary>
        /// Identificador del rol.
        /// </summary>
        [JsonPropertyName("id")]
        required public string Id { get; set; }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [JsonPropertyName("name")]
        required public string Name { get; set; }
    }
}