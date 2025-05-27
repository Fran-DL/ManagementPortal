using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Dto de permisos.
    /// </summary>
    public class PermissionGetDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionGetDto"/> class.
        /// </summary>
        public PermissionGetDto()
        {
            this.Name = string.Empty;
        }

        /// <summary>
        /// Id unico para el permiso.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del permiso.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}