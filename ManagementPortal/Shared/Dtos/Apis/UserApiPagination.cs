using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase Utilizada para el Get de Usuarios (LISTADO).
    /// </summary>
    public class UserApiPagination
    {
        /// <summary>
        /// Listado de los usuarios.
        /// </summary>
        [JsonPropertyName("results")]
        required public List<UserBasicInfo> Results { get; set; }

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