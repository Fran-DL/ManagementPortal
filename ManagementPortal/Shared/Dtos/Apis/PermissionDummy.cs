namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Clase que se utiliza para almacenar la info de los permisos de los Productos.
    /// </summary>
    public class PermissionDummy
    {
        /// <summary>
        /// Identificador del permiso en el producto X.
        /// </summary>
        required public int Id { get; set; }

        /// <summary>
        /// Nombre del permiso en el producto X.
        /// </summary>
        required public string Name { get; set; }
    }
}