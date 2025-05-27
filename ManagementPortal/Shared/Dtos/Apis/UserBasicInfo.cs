using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase que se retorna en el Listado de Uusarios con la información básica de este.
    /// </summary>
    public class UserBasicInfo
    {
        /// <summary>
        /// Identificador del Usuario en el producto X.
        /// </summary>
        [JsonPropertyName("id")]
        required public string Id { get; set; }

        /// <summary>
        /// Nombre de Usuario en el producto X.
        /// </summary>
        [JsonPropertyName("userName")]
        required public string UserName { get; set; }

        /// <summary>
        /// Email del usuario en el producto X.
        /// </summary>
        [JsonPropertyName("email")]
        required public string Email { get; set; }

        /// <summary>
        /// Nombre del usuario en el producto X.
        /// </summary>
        [JsonPropertyName("name")]
        required public string Name { get; set; }

        /// <summary>
        /// Apellido del usuario en el producto X.
        /// </summary>
        [JsonPropertyName("lastName")]
        required public string LastName { get; set; }

        /// <summary>
        /// Lista de roles del usuario en el producto X.
        /// </summary>
        [JsonPropertyName("roles")]
        required public List<RoleBasicInfo> Roles { get; set; }
    }
}