using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// StatusDto.
    /// </summary>
    public class StatusDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusDto"/> class.
        /// </summary>
        public StatusDto()
        {
            Id = 0;
            Name = string.Empty;
            Color = string.Empty;
        }

        /// <summary>
        /// Id.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Color.
        /// </summary>
        [JsonPropertyName("color")]
        public string Color { get; set; }
    }
}