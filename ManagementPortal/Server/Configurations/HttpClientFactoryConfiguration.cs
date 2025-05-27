using ManagementPortal.Server.Configurations.Parsers;
using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para parametrizar la configuracion del HttpClientFactory.
    /// </summary>
    public static class HttpClientFactoryConfiguration
    {
        /// <summary>
        /// Metodo que parametriza la configuracion del HttpClientFactory en la aplicacion.
        /// </summary>
        /// <param name="services">Interfaz de servicios de la aplicacion.</param>
        /// <param name="configuration">Interfaz para poder tomar parametros de appsettings.</param>
        /// <param name="env">Interfaz par determinar entorno donde ejecuta la API.</param>
        /// <param name="webhost">Host del builder donde ejecuta laa API.</param>
        public static void ConfigureHttpFactory(
            this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, ConfigureWebHostBuilder webhost)
        {
            {
                var products = configuration.GetSection("Products").Get<List<ProductSettings>>();
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        services.AddHttpClient(product.Name, client =>
                        {
                            if (env.IsDevelopment() && configuration.GetValue<bool>("SeedDummys"))
                            {
                                var baseUrl = ApplicationUrl.ParseUrl(configuration.GetValue<string>("applicationUrl") ?? string.Empty) ?? ApplicationUrl.Localhost + '/';
                                product.Url = baseUrl + product.Name + '/' + "api" ?? string.Empty;
                            }

                            client.BaseAddress = new Uri(product.Url);
                            client.DefaultRequestHeaders.Add("X-API-KEY", product.ApiKey);
                        });
                    }
                }
            }
        }
    }
}