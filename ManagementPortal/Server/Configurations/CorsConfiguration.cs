namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para parametrizar Cors en la aplicacion.
    /// </summary>
    public static class CorsConfiguration
    {
        /// <summary>
        /// Metodo que implementa la parametrizacion de Cors en la aplicaicon.
        /// </summary>
        /// <param name="services">Interfaz de servicios de la aplicacion.</param>
        /// <param name="configuration">Interfaz para obtener parametros de appsettings.</param>
        /// <param name="corsPolicy">Para asignas un nombre a la polica de Cors.</param>
        public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration, string corsPolicy)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    name: corsPolicy,
                    policy =>
                    {
                        policy.WithOrigins(configuration.GetSection("CORS").Get<string[]>()!)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
        }
    }
}