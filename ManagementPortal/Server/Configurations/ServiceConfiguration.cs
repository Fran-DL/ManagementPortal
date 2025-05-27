using System.Globalization;
using ManagementPortal.Server.Services;
using Microsoft.AspNetCore.Localization;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para agregar servicios a la aplicacion.
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Metodo que agrega los servicios desarrollados a la aplicacion.
        /// </summary>
        /// <param name="services">Interfaz para poder agregar los servicios en la aplicacion.</param>
        public static void ConfigureCustomServices(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddScoped<Services.MessagingService>();
            services.AddScoped<TokenService>();
            services.AddScoped<TwoFactorAuthService>();
            services.AddTransient<EmailService>();
            services.AddSingleton<OtpDummyService>();

            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("es-ES"),
                    new CultureInfo("pt-BR"),
                };

                options.DefaultRequestCulture = new RequestCulture("es-ES");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
        }
    }
}