using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using Microsoft.AspNetCore.Components.Authorization;

namespace ManagementPortal.Client.Services
{
    /// <summary>
    /// Componente que se encarga de manejar el estado de la sesion del usuario.
    /// Justificacion: La sesion de usuario se valida/modifica en toda la aplicacion.
    /// Se requiere un componente para agrupar funcionalidad.
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthStateProvider"/> class.
        /// Inicializa una nueva instancia de la clase <see cref="CustomAuthStateProvider"/>.
        /// </summary>
        /// <param name="localStorage">Interfaz para poder trabajar con local storage.</param>
        /// <param name="httpClientFactory">Interfaz para poder unificar logica al enviar solicitudes al server.</param>
        public CustomAuthStateProvider(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory)
        {
            this._localStorage = localStorage;
            this._httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Evento para notificar el estado de autenticacion.
        /// </summary>
        public event Action<bool>? AuthStateChanged;

        /// <summary>
        /// Metodo para implementar el logout de la aplicacion (solo borra el local storage).
        /// </summary>
        /// <returns>No retorna nada, se implementa como un Task.</returns>
        public async Task Logout()
        {
            await this._localStorage.RemoveItemAsync("token");
            await this._localStorage.RemoveItemAsync("refreshToken");

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }

        /// <summary>
        /// Metodo para notificar sobre el cambio en estado de autenticacion.
        /// </summary>
        /// <param name="isAuthenticated">Indica si el usuario se encuentra autenticado.</param>
        public async void NotifyTokenExpired(bool isAuthenticated)
        {
            await Logout();
            AuthStateChanged?.Invoke(isAuthenticated);
        }

        /// <summary>
        /// Obtiene el estado de la sesion del usuario. Mira en local storage, valida fechas de expiracion y sesion con servidor.
        /// </summary>
        /// <returns>Retorna el estado de autenticacion.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await this._localStorage.GetItemAsync<string?>("token");
            var identity = new ClaimsIdentity();

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);

            var expiryUnix = long.Parse(jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value ?? "0");
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).UtcDateTime;

            if (expiryDateTime < DateTime.UtcNow)
            {
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }

            var isValid = await this.ValidateAuthOnServerAsync();
            if (!isValid)
            {
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }

            var claims = jwtToken.Claims;

            identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        /// <summary>
        /// Metodo que se encarga de guardar el Jwt y Refresh Token en local storage. Notifica el nuevo estado de autenticacion.
        /// </summary>
        /// <param name="authenticationResult">Jwt y Refresh Token del usuario.</param>
        /// <returns>No retorna nada ya que se implementa como Task.</returns>
        public async Task NotifyUserAuthentication(AuthenticationResult authenticationResult)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(authenticationResult.Token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));

            await this._localStorage.SetItemAsync("token", authenticationResult.Token);
            await this._localStorage.SetItemAsync("refreshToken", authenticationResult.RefreshToken);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
        }

        /// <summary>
        /// Metodo auxiliar para validar la sesion de usuario en el servidor.
        /// </summary>
        /// <returns>Retorna true si el usuario se encuentra logueado.</returns>
        private async Task<bool> ValidateAuthOnServerAsync()
        {
            var client = this._httpClientFactory.CreateClient("CustomHttpClient");
            client.DefaultRequestHeaders.Add("product", Products.ManagementPortal.ToString());
            var response = await client.GetAsync("api/Account/validate");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
    }
}