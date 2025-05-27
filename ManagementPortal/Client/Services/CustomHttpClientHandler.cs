using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace ManagementPortal.Client.Services
{
    /// <summary>
    /// Handler que se encarga de setear el Jwt en cada solicitud al servidor.
    /// Justificacion: Se requiere agrupar la logica de comunicacion con el server en un unico componente ya que se utiliza en
    /// toda la aplicacion.
    /// </summary>
    public class CustomHttpClientHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly CustomAuthStateProvider _customAuthStateProvider;
        private readonly NavigationManager _navigationManager;
        private IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomHttpClientHandler"/> class.
        /// </summary>
        /// <param name="localStorage">Interfaz para implementar manejo de local storage.</param>
        /// <param name="customAuthStateProvider">Servicio de autenticacion para revisar token.</param>
        /// <param name="navigationManager">Servicio para manejar navegación.</param>
        /// <param name="httpClientFactory">Interfaz para poder unificar logica al enviar solicitudes al server.</param>
        public CustomHttpClientHandler(
            ILocalStorageService localStorage,
            CustomAuthStateProvider customAuthStateProvider,
            NavigationManager navigationManager,
            IHttpClientFactory httpClientFactory)
        {
            _localStorage = localStorage;
            _customAuthStateProvider = customAuthStateProvider;
            _navigationManager = navigationManager;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Metodo que se encarga de obtener el token del local storage y agregarlo al header en cada solicitud al servidor.
        /// </summary>
        /// <param name="request">Request que se envia desde la aplicacion al servidor.</param>
        /// <param name="cancellationToken">Parametro por defecto que no se utiliza en este contexto.</param>
        /// <returns>Devuelve la respuesta Http del servidor.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null && !request.RequestUri.AbsolutePath.Contains("/login") && !request.RequestUri.AbsolutePath.Contains("/RefreshToken"))
            {
                var token = await _localStorage.GetItemAsync<string>("token");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                request.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var expirationTime = jwtToken.ValidTo;
                    var now = DateTime.UtcNow;
                    if (expirationTime < now)
                    {
                        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            var refreshResult = await RefreshToken(refreshToken);

                            if (refreshResult != null)
                            {
                                await _customAuthStateProvider.NotifyUserAuthentication(refreshResult);

                                token = refreshResult.Token;
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            }
                            else
                            {
                                _customAuthStateProvider.NotifyTokenExpired(false);

                                // return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                            }
                        }
                        else
                        {
                            _customAuthStateProvider.NotifyTokenExpired(false);

                            // return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<AuthenticationResult?> RefreshToken(string refreshToken)
        {
            try
            {
                var httpClient = this._httpClientFactory.CreateClient("CustomHttpClient");
                httpClient.DefaultRequestHeaders.Add("product", Products.ManagementPortal.ToString());
                var response = await httpClient.PostAsJsonAsync("api/Account/RefreshToken", refreshToken);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
                    return result;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}