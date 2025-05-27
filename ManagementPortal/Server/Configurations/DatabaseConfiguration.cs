using ManagementPortal.Server.Context;
using Microsoft.EntityFrameworkCore;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para parametrizar la base de datos de la aplicacion.
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Metodo que implementa la parametrizacion de la base de datos en la aplicacion.
        /// </summary>
        /// <param name="services">Interfaz de servicios de la aplicacion.</param>
        /// <param name="configuration">Interfaz para obtener los parametros de configuracion (connectionString).</param>
        /// <param name="environment">Interfaz para determinar ambiente y setear el gesto de db correspondiente.</param>
        public static void ConfigureDatabase(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
        }
    }
}