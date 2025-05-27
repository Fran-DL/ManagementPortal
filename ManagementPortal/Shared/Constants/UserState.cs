namespace ManagementPortal.Shared.Constants
{
    /// <summary>
    /// Enumerado para elegir el estado de los usuarios en el filtrado.
    /// </summary>
    public enum UserState
    {
        /// <summary>
        /// Se ven solo usuarios activos.
        /// </summary>
        Active,

        /// <summary>
        /// Se ven solo usuarios borrados.
        /// </summary>
        Deleted,

        /// <summary>
        /// Se ven todos los usuarios.
        /// </summary>
        Both,
    }
}