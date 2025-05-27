using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase que se utiliza para almacenar la info de los usuarios.
    /// </summary>
    public class UserDummy
    {
        /// <summary>
        /// ID del usuario.
        /// </summary>
        required public string Id { get; set; }

        /// <summary>
        /// Producto al que pertenece el usuario.
        /// </summary>
        required public Products Producto { get; set; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        required public string UserName { get; set; }

        /// <summary>
        /// Email del usuario.
        /// </summary>
        required public string Email { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        required public string Name { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        required public string LastName { get; set; }

        /// <summary>
        /// Password del usuario.
        /// </summary>
        required public string Password { get; set; }

        /// <summary>
        /// Status del usuario.
        /// </summary>
        required public Status Status { get; set; }

        /// <summary>
        /// Lista de Roles del usuario.
        /// </summary>
        required public List<RoleDummy> Roles { get; set; }

        /// <summary>
        /// Imagen del usuario.
        /// </summary>
        required public string? Image { get; set; }

        /// <summary>
        /// Firma del usuario.
        /// </summary>
        public byte[]? Signature { get; set; }

        /// <summary>
        /// Estado del usuario.
        /// </summary>
        required public bool Active { get; set; }

        /// <summary>
        /// TenantId del usuario.
        /// </summary>
        required public int TenantId { get; set; }

        /// <summary>
        /// LastVersion del usuario.
        /// </summary>
        required public string LastVersion { get; set; }

        /// <summary>
        /// ExternalIds del usuario.
        /// </summary>
        required public string ExternalIds { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Id: {Id}, UserName: {UserName}, Email: {Email}, Name: {Name}, LastName: {LastName}, Active: {Active}, TenantId: {TenantId}, LastVersion: {LastVersion}, ExternalIds: {ExternalIds}";
        }
    }
}