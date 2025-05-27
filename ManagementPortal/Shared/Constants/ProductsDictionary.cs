namespace ManagementPortal.Shared.Constants
{
    /// <summary>
    /// Clase que implementa el diccionario de mapeo de los valores del enum Products a sus representaciones en string.
    /// </summary>
    public class ProductsDictionary
    {
        /// <summary>
        /// Diccionario que mapea los valores del enum Products a sus representaciones en string.
        /// </summary>
        public static readonly Dictionary<Products, string> ProductDictionary = new()
        {
            { Products.ManagementPortal, "Management Portal" },
            { Products.AssetManager, "Asset Manager" },
            { Products.IoTMonitor, "IoT Monitor" },
            { Products.EventManager, "Event Manager" },
            { Products.OthersProducts, "Otros Productos" },
        };
    }
}