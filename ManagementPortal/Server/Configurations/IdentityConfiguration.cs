using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para la parametrizacion de Identity en la aplicacion.
    /// </summary>
    public static class IdentityConfiguration
    {
        /// <summary>
        /// Metodo que implementa la parametrizacion de identity en la aplicacion.
        /// </summary>
        /// <param name="services">Interfaz de servicios de la aplicacion.</param>
        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();
        }
    }
}