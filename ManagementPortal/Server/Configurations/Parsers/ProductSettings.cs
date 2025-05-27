namespace ManagementPortal.Server.Configurations.Parsers
{
    /// <summary>
    /// Clase que se implementa para representar la configuracion de un producto del sistema.
    /// </summary>
    public class ProductSettings
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        required public string Name { get; set; }

        /// <summary>
        /// Url del producto.
        /// </summary>
        required public string Url { get; set; }

        /// <summary>
        /// ApiKey necesaria para acceder a las apis del producto.
        /// </summary>
        required public string ApiKey { get; set; } // Es recomendable considerar medidas de seguridad para manejar la ApiKey

        /// <summary>
        /// SharedKey necesaria para acceder a las apis del producto.
        /// </summary>
        required public string SharedKey { get; set; } // Es recomendable considerar medidas de seguridad para manejar la ApiKey
    }
}