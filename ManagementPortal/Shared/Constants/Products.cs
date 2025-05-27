namespace ManagementPortal.Shared.Constants
{
    /// <summary>
    /// Enumerado que identifica las aplicaciones a manejar por el Management Portal.
    /// Justificacion: Se requiere implementar roles y permisos por aplicacion.
    /// </summary>
    public enum Products
    {
        /// <summary>
        /// Enumerado que identifica la aplicacion Management Portañ.
        /// </summary>
        ManagementPortal,

        /// <summary>
        /// Enumerado que identifica la aplicacion de Activos.
        /// </summary>
        AssetManager,

        /// <summary>
        /// Enumerado que identifica la aplicacion de IoT Monitor.
        /// </summary>
        IoTMonitor,

        /// <summary>
        /// Enumerado que identifica la aplicacion de Event Manager.
        /// </summary>
        EventManager,

        /// <summary>
        /// Enumerado que identifica la aplicacion de Otros Productos.
        /// </summary>
        OthersProducts,
    }
}