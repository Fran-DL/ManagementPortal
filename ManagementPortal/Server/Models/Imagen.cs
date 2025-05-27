namespace ManagementPortal.Server.Models
{
    /// <summary>
    /// Clase utilizada para subir imagenes al servidor.
    /// </summary>
    public class Imagen
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Imagen"/> class.
        /// </summary>
        public Imagen()
        {
            this.Nombre = string.Empty;
            this.File = null!; // Use null-forgiving operator to suppress the warning
        }

        /// <summary>
        /// Nombre del archivo.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Archivo a subir.
        /// </summary>
        public IFormFile File { get; set; }
    }
}