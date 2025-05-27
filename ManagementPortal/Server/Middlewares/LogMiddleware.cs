using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
using ManagementPortal.Shared.Resources;
using ManagementPortal.Shared.Resources.Server;
using Serilog;
using Serilog.Context;

namespace ManagementPortal.Server.Middlewares
{
    /// <summary>
    /// Middleware para registrar solicitudes a la API.
    /// </summary>
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMiddleware"/> class.
        /// </summary>
        /// <param name="next">Request delegate para el siguiente middleware a ejecutar.</param>
        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Metodo para procesar la solicitud que dispara.
        /// </summary>
        /// <param name="context">Http context.</param>
        /// <returns>No retorna valor.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string product = string.Empty;

            if (context.Request.Headers.TryGetValue("Authorization", out var extractedToken))
            {
                var token = extractedToken.ToString().Replace("Bearer ", string.Empty);
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    product = jwtToken.Audiences.First();
                }
            }

            if (product.Length == 0 && context.Request.Headers.TryGetValue("product", out var extractedProduct))
            {
                product = extractedProduct.ToString();
            }

            if (product.Length == 0)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(LoggingResources.ProductRequired);
                return;
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var request = $"{context.Request.Method} {context.Request.Path.Value}";
            string user = string.Empty;
            string body = string.Empty;

            context.Request.EnableBuffering();

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            string pattern = @"ssword\"":\""[^\""]*\""";
            string replacement = "ssword\":\"[REDACTED]\"";
            body = Regex.Replace(body, pattern, replacement);

            if (context.User.Identity?.Name != null)
            {
                user = context.User.Identity.Name;
            }

            string header = string.Empty;
            if (!string.IsNullOrEmpty(context.Request.QueryString.Value))
            {
                header = context.Request.QueryString.Value;
            }

            try
            {
                if (string.IsNullOrEmpty(product))
                {
                    Log.Warning(string.Format(LogErrorResources.LogFieldsEmpty, nameof(product)));
                }

                if (string.IsNullOrEmpty(ipAddress))
                {
                    Log.Warning(string.Format(LogErrorResources.LogFieldsEmpty, nameof(ipAddress)));
                }

                if (string.IsNullOrEmpty(user) && !request.Contains("POST /api/Account/login"))
                {
                    Log.Warning(string.Format(LogErrorResources.LogFieldsEmpty, nameof(user)));
                }

                if (string.IsNullOrEmpty(request))
                {
                    Log.Warning(string.Format(LogErrorResources.LogFieldsEmpty, nameof(request)));
                }

                using (LogContext.PushProperty("Application", product))
                using (LogContext.PushProperty("IpAddress", ipAddress))
                using (LogContext.PushProperty("UserId", user))
                using (LogContext.PushProperty("Action", request))
                {
                    if (!string.IsNullOrEmpty(body))
                    {
                        Log.Information($"{request} : {body}");
                    }
                    else
                    {
                        Log.Information($"{request} : {header}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format(LogErrorResources.LogPropertiesError, ex));
            }

            await _next(context);
        }
    }
}