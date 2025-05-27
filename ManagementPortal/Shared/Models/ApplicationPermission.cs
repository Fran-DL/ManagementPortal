using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ManagementPortal.Shared.Models
{
    /// <summary>
    /// Clase que implementa la entidad permiso para los usuarios.
    /// Justificacion: Se necesita asignar permisos para los usuarios además de roles.
    /// </summary>
    public class ApplicationPermission
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationPermission"/> class.
        /// </summary>
        public ApplicationPermission()
        {
            this.IsDeleted = false;
            this.Name = string.Empty;
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
        /// Roles que posen el permiso.
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<ApplicationRole>? Roles { get; set; }

        /// <summary>
        /// Indica si la instancia fue eliminada.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}