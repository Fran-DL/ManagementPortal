using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para los filtrados.
    /// </summary>
    public class FiltradosDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FiltradosDto"/> class.
        /// </summary>
        public FiltradosDto()
        {
            Product = Products.ManagementPortal;
            CurrentPage = 1;
            PageSize = 10;
            SearchText = string.Empty;
            SortOrder = Order.Ascending;
        }

        /// <summary>
        /// Producto.
        /// </summary>
        public Products Product { get; set; }

        /// <summary>
        /// Pagina actual.
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Tamaño de la pagina.
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Texto que se busca en el filtrado.
        /// </summary>
        public string SearchText { get; set; } = string.Empty;

        /// <summary>
        /// Ascending o Descending.
        /// </summary>
        public Order SortOrder { get; set; } = Order.Ascending;
    }
}