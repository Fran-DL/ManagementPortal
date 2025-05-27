namespace ManagementPortal.Shared.Dtos.Apis
{
    /// <summary>
    /// Enum para manejar los Status.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Disponible.
        /// </summary>
        Disponible = 1,

        /// <summary>
        /// Ocupado.
        /// </summary>
        Ocupado = 2,

        /// <summary>
        /// Desconectado.
        /// </summary>
        Desconectado = 3,

        /// <summary>
        /// En Servicio.
        /// </summary>
        EnServicio = 4,

        /// <summary>
        /// En Mantenimiento.
        /// </summary>
        EnMantenimiento = 5,

        /// <summary>
        /// Desconocido.
        /// </summary>
        Desconocido = 0,
    }
}