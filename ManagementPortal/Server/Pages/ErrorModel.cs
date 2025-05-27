using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ManagementPortal.Server.Pages
{
    /// <summary>
    /// Pagina que utiliza el modelo de error.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// Ilogger para el modelo de error.
        /// </summary>
        private readonly ILogger<ErrorModel> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorModel"/> class.
        /// </summary>
        /// <param name="logger"> Ilogger para el modelo de error. </param>
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            this.RequestId = string.Empty;
            this._logger = logger;
        }

        /// <summary>
        /// Obtiene o establece el identificador de la solicitud.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Se obtiene un valor que indica si se debe mostrar el identificador de la solicitud.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Funcion que se ejecuta al cargar la pagina.
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}