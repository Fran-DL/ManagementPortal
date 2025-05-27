using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ManagementPortal.Server.Configurations
{
    /// <summary>
    /// Clase que se implementa para parametrizar manejo de Jwt en la aplicacion.
    /// </summary>
    public static class AuthenticationConfiguration
    {
        /// <summary>
        /// Metodo que parametriza Jwt en la aplicacion.
        /// </summary>
        /// <param name="services">Interfaz de servicios de la aplicacion.</param>
        /// <param name="configuration">Interfaz para poder tomar parametros de appsettings.</param>
        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var key = configuration["Jwt:Key"];
            var issuer = configuration["Jwt:Issuer"];
            var audiences = configuration.GetSection("Jwt:Audiences").Get<string[]>();

            if (key == null || issuer == null || audiences == null)
            {
                ArgumentNullException argumentNullException = new("JWT configuration values cannot be null");
                throw argumentNullException;
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudiences = audiences,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    NameClaimType = ClaimTypes.NameIdentifier,
                };
            });

            services.AddControllers();
        }
    }
}