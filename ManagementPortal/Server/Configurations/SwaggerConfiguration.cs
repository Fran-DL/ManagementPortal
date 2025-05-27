using Microsoft.OpenApi.Models;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se encarga de configurar swagger.
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Metodo que se encarga de setear los parametros de configuracion para swagger.
        /// </summary>
        /// <param name="services">Interfaz para poder utilizar los servicios de la aplicacion.</param>
        /// <param name="configuration">Interfaz para poder obtener parametros del appsettings.</param>
        public static void ConfigureSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "ManagementPortal", Version = configuration["Version"] });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Ingresa el token JWT en el siguiente formato: **Bearer {token}**",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                });
            });
        }
    }
}