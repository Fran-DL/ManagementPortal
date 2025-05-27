using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace ManagementPortal.Server.Middlewares
{
    /// <summary>
    /// Middleware que implementa log al momento de exception.
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">Request delegate para el siguiente middleware a ejecutar.</param>
        public ExceptionHandlerMiddleware(RequestDelegate next)
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
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (!context.Response.HasStarted)
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

                    if (context.User.Identity?.Name != null)
                    {
                        user = context.User.Identity.Name;
                    }

                    using (LogContext.PushProperty("Application", product))
                    using (LogContext.PushProperty("IpAddress", ipAddress))
                    using (LogContext.PushProperty("UserId", user))
                    using (LogContext.PushProperty("Action", request))
                    {
                        Log.Error(ex.Message);
                    }

                    await HandleExceptionAsync(context, ex);
                }
                else
                {
                    // IMPLEMENTAR LOG, EL CONTEXT TIENE LA INFO QUE NECESITAMOS.
                }
            }
        }

        /// <summary>
        /// Metodo para estandarizar respuesta de API en caso de exception.
        /// </summary>
        /// <param name="context">Http context.</param>
        /// <param name="exception">Exception que dispara.</param>
        /// <returns>Retonar un json de detalle del error.</returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var result = JsonConvert.SerializeObject(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error",
                Detailed = exception.Message ?? string.Empty,
            });

            return context.Response.WriteAsync(result);
        }
    }
}