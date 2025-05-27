using ManagementPortal.Shared.Constants;

namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para los firltados de log.
    /// </summary>
    public class FilterLogDto : FiltradosDto
    {
        /// <summary>
        /// Campo por el que se quiere ordenar.
        /// </summary>
        public SortFieldLog? SortField { get; set; }

        /// <summary>
        /// Campo de filtrado por fecha de comienzo de log.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Campo de filtrado por fecha de finalizacion de log.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}