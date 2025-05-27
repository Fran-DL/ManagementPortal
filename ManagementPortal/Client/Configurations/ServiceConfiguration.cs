using Blazored.LocalStorage;
using ManagementPortal.Client.Themes;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Services;

namespace ManagementPortal.Client.Services
{
    /// <summary>
    /// Clase que se encarga de agrupar los servicios implementados en la aplicacion.
    /// Justificacion: Permite centralizar la gestion de servicios. No queremos sobrecargar el program.cs.
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Metodo que se encarga de agregar a la aplicacion los distintos servicios.
        /// </summary>
        /// <param name="services">Interfaz para poder manipular los servicios.</param>
        public static void ConfigureCustomServices(
            this IServiceCollection services)
        {
            services.AddBlazoredLocalStorageAsSingleton();
            services.AddAuthorizationCore();
            services.AddSingleton<CustomAuthStateProvider>();
            services.AddSingleton<AuthenticationStateProvider, CustomAuthStateProvider>();
            services.AddScoped<CustomHttpClientHandler>();
            services.AddScoped<CustomThemeProvider>();
            services.AddSingleton<MessagingService>();
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
            });
        }
    }
}