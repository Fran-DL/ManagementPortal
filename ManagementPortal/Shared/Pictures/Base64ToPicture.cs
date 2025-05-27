namespace ManagementPortal.Shared.Pictures
{
    /// <summary>
    /// Clase para manejar la conversión de base64 a imagen.
    /// </summary>
    public class Base64ToPicture
    {
        /// <summary>
        /// Convierte una imagen en base64 a un string que puede ser usado en un tag img de html.
        /// </summary>
        /// <param name="base64String">Imagen en base64.</param>
        /// <returns>Imagen en base64 con el formato adecuado para ser usado en un tag img de html.</returns>
        public static string? ConvertToWebPicture(string? base64String)
        {
            if (base64String == null || base64String.Length == 0)
            {
                return "data:image/png;base64," + Convert.ToBase64String(ManagementPortal.Shared.Resources.ApiSONDA.UserDummyPictures.No_picture);
            }
            else if (base64String.StartsWith("iVBORw0KGgo"))
            {
                return "data:image/png;base64," + base64String;
            }
            else if (base64String.StartsWith("/9j/"))
            {
                return "data:image/jpg;base64," + base64String;
            }
            else
            {
                return "data:image/png;base64," + Convert.ToBase64String(ManagementPortal.Shared.Resources.ApiSONDA.UserDummyPictures.No_picture);
            }
        }
    }
}