using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// DTO de roles utilizado en el Get de Roles.
    /// </summary>
    public class RoleGetDto
    {
        /// <summary>
        /// Id del rol.
        /// </summary>
        [JsonPropertyName("id")]
        required public string Id { get; set; }

        /// <summary>
        /// Nombre del rol.
        /// </summary>
        [JsonPropertyName("name")]
        required public string Name { get; set; }

        /// <summary>
        /// Permisos que pertenecen al rol.
        /// </summary>
        [JsonPropertyName("permissions")]
        required public List<PermissionGetDto> Permissions { get; set; }
    }
}