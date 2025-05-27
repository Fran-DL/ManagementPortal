using System.ComponentModel.DataAnnotations;
using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto de permisos.
    /// Justificacion: Se necesita un dto para transportar permisos.
    /// </summary>
    public class ApplicationPermissionDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationPermissionDto"/> class.
        /// </summary>
        public ApplicationPermissionDto()
        {
            this.Name = string.Empty;
            this.ApplicationManagment = Products.OthersProducts;
        }

        /// <summary>
        /// Id unico para el permiso.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del permiso.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Producto al que pertence el permiso.
        /// </summary>
        public Products ApplicationManagment { get; set; }
    }
}