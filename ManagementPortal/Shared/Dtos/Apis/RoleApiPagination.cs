using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase que se implementa la paginacion de los roles.
    /// </summary>
    public class RoleApiPagination
    {
        /// <summary>
        /// Listado de los usuarios.
        /// </summary>
        [JsonPropertyName("results")]
        required public List<RoleGetDto> Results { get; set; }

        /// <summary>
        /// Mensaje de error.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        required public string? ErrorMessage { get; set; }

        /// <summary>
        /// Pagina actual.
        /// </summary>
        [JsonPropertyName("currentPage")]
        required public int CurrentPage { get; set; }

        /// <summary>
        /// Total de items.
        /// </summary>
        [JsonPropertyName("totalItems")]
        required public int TotalItems { get; set; }

        /// <summary>
        /// Total de paginas.
        /// </summary>
        [JsonPropertyName("totalPages")]
        required public int TotalPages { get; set; }
    }
}