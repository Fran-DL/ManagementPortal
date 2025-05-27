using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Dto de roles.
    /// </summary>
    public class RolePagination
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RolePagination"/> class.
        /// </summary>
        public RolePagination()
        {
            this.Result = new List<RoleGetDto>();
            this.CurrentPage = 1;
            this.TotalItems = 0;
            this.TotalPages = 0;
            this.ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Lista de roles.
        /// </summary>
        [JsonPropertyName("results")]
        public List<RoleGetDto> Result { get; set; }

        /// <summary>
        /// Mensaje de error.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Pagina actual.
        /// </summary>
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// Total de roles que cumplen los filtros.
        /// </summary>
        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        /// <summary>
        /// Total de paginas.
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }
}