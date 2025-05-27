namespace ManagementPortal.Shared.Dtos
{
    /// <summary>
    /// Dto para combinar el Otp con el UserId asociado.
    /// </summary>
    public class OtpRequestDto
    {
        /// <summary>
        /// Otp del dto.
        /// </summary>
        public string? Otp { get; set; }

        /// <summary>
        /// UserId relacionado al Otp.
        /// </summary>
        public string? UserId { get; set; }
    }
}