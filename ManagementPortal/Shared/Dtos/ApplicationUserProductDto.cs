using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos.Apis;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para transportar la información del usuario en otros productos.
    /// Justificación: Se necesita transportar la información del usuario que no se almacena en MP.
    /// </summary>
    public class ApplicationUserProductDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserProductDto"/> class.
        /// </summary>
        public ApplicationUserProductDto()
        {
            Status = new StatusDto();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUserProductDto"/> class.
        /// </summary>
        /// <param name="existing">Dto para realizar la copia.</param>
        public ApplicationUserProductDto(ApplicationUserProductDto existing)
        {
            Product = existing.Product;
            ProductDelete = existing.ProductDelete;
            Active = existing.Active;
            TenantId = existing.TenantId;
            Status = existing.Status;
            LastVersion = existing.LastVersion;
            ExternalIds = existing.ExternalIds;
            Signature = existing.Signature;
        }

        /// <summary>
        /// Producto del usuario.
        /// </summary>
        public Products Product { get; set; }

        /// <summary>
        /// Booleano que permite controlar si la relacion usuario-producto existio pero fue borrada.
        /// Si es true, quiere decir que el usuario tuvo el producto y fue dado de baja.
        /// </summary>
        public bool ProductDelete { get; set; } = false;

        /// <summary>
        /// Indica si el usuario se encuentra activo o dado de baja (borrado lógico).
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Tenant al que el usuario pertenece.
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Estado actual del usuario (disponible, ocupado, desconectado, etc).
        /// </summary>
        public StatusDto Status { get; set; }

        /// <summary>
        /// Última versión del producto a la que accedió el usuario.
        /// </summary>
        public string? LastVersion { get; set; }

        /// <summary>
        /// Identificador del usuario en otros sistemas de terceros.
        /// </summary>
        public string? ExternalIds { get; set; }

        /// <summary>
        /// Firma del usuario (en base 64).
        /// </summary>
        public string? Signature { get; set; }
    }
}