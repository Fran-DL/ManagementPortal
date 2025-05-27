using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ManagementPortal.Server.Context;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ManagementPortal.Server.Services
{
    /// <summary>
    /// Servicio que implementa la generación de Jwt y Refresh Token.
    /// Justificacion: Se implementan las funcionalidades asociadas a la generacion de Tokens para agrupar ya que se utilizan
    /// en varios metodos/controllers.
    /// </summary>
    public class TokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationContext _applicationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="userManager">Manejador de Identity para Usuarios.</param>
        /// <param name="configuration">Configuracion para tomar parametros de appsettings.</param>
        /// <param name="applicationContext">EF para manejar la base de datos de la aplicacion.</param>
        /// <param name="roleManager">Manejador de Identity para Roles.</param>
        public TokenService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationContext applicationContext,
            RoleManager<ApplicationRole> roleManager)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._applicationContext = applicationContext;
            this._roleManager = roleManager;
        }

        /// <summary>
        /// Genera la pareja (Jwt, RefreshToken) para el usuario dado.
        /// </summary>
        /// <param name="user">Usuario de Identity.</param>
        /// <param name="product">Producto para asignar audience al token.</param>
        /// <returns>Retorna la pareja (Jwt, RefreshToken).</returns>
        public async Task<(string AccessToken, string RefreshToken)> GenerateTokens(ApplicationUser user, Products product)
        {
            var accessToken = await this.GenerateAccessToken(user, product);
            var refreshToken = Guid.NewGuid().ToString();

            await this._userManager.SetAuthenticationTokenAsync(user, "Default", "Jwt", accessToken);

            user.RefreshToken = refreshToken;
            user.ResfreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await this._userManager.UpdateAsync(user);

            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Metodo que se encarga de refrescar el Jwt y el Refresh Token.
        /// </summary>
        /// <param name="refreshToken">refresh token.</param>
        /// <param name="product">Producto para asignar audience al token.</param>
        /// <returns>Nuevos tokens.</returns>
        public async Task<(string AccessToken, string RefreshToken)> RefreshTokens(string refreshToken, Products product)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.ResfreshTokenExpiry <= DateTime.UtcNow)
            {
                throw new SecurityTokenException();
            }

            var newTokens = await GenerateTokens(user, product);

            return newTokens;
        }

        /// <summary>
        /// Metodo auxiliar para generar el Jwt Token.
        /// </summary>
        /// <param name="user">Usuario de Identity al que se asigna el Jwt.</param>
        /// <param name="product">Producto para asignar audience al token.</param>
        /// <returns>Retonar el Jwt para el usuario de Identity.</returns>
        private async Task<string> GenerateAccessToken(ApplicationUser user, Products product)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
            };

            var roles = await _roleManager.Roles.Include(r => r.Permissions).ToListAsync();
            var userNameRoles = await _userManager.GetRolesAsync(user);

            List<ApplicationRole> userRoles = roles.Where(r => userNameRoles != null && userNameRoles.Contains(r.Name ?? string.Empty))
                .ToList();

            List<string> permissionNames = new();
            foreach (var role in userRoles ?? new List<ApplicationRole>())
            {
                foreach (var permission in role.Permissions ?? new List<ApplicationPermission>())
                {
                    permissionNames.Add(permission.Name);
                }
            }

            List<string> permissionNamesUnique = permissionNames.Distinct().ToList();
            foreach (string permissionName in permissionNamesUnique)
            {
                claims.Add(new Claim(ClaimTypes.Role, permissionName));
            }

            if (this._configuration == null)
            {
                throw new ArgumentNullException(nameof(this._configuration));
            }

            var jwtKey = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key configuration is missing.");
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiry = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: this._configuration["Jwt:Issuer"],
                audience: product.ToString(),
                claims: claims,
                expires: expiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}