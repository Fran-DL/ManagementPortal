namespace ManagementPortal.Shared.Constants
{
    /// <summary>
    /// Url de la aplicación.
    /// </summary>
    public static class ApplicationUrl
    {
        /// <summary>
        /// Url de Localhost.
        /// </summary>
        public static readonly string Localhost = "https://localhost:7109/";

        /// <summary>
        /// Parsea la url de la aplicación.
        /// </summary>
        /// <param name="args">String.</param>
        /// <returns>Parsed URL string if valid; otherwise, null.</returns>
        public static string? ParseUrl(string args)
        {
            if (Uri.TryCreate(args, UriKind.RelativeOrAbsolute, out var url) && !string.IsNullOrEmpty(args))
            {
                var uri = url.ToString();

                if (string.IsNullOrEmpty(url.ToString()))
                {
                    return null;
                }

                return args;
            }

            return null;
        }
    }
}