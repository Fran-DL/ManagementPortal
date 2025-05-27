using System.Globalization;
using Microsoft.JSInterop;

namespace ManagementPortal.Client.Configurations
{
    /// <summary>
    /// Clase encargada de configurar la cultura de la aplicacion.
    /// </summary>
    public static class CultureConfiguration
    {
        /// <summary>
        /// Metodo que se encarga de configurar la cultura de la aplicacion.
        /// </summary>
        /// <param name="services"> Interfaz para poder utilizar los servicios de la aplicacion.</param>
        /// <returns> Task que representa la operacion asincrona.</returns>
        public static async Task ConfigureCulture(this IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            const string defaultCulture = "es-ES";
            var js = services.BuildServiceProvider().GetRequiredService<IJSRuntime>();
            var result = await js.InvokeAsync<string>("blazorCulture.get");
            var culture = CultureInfo.GetCultureInfo(result ?? defaultCulture);

            if (result == null)
            {
                await js.InvokeVoidAsync("blazorCulture.set", defaultCulture);
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}