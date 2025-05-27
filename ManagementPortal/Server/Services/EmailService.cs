using MailKit.Net.Smtp;
using ManagementPortal.Shared.Models;
using ManagementPortal.Shared.Resources.Server;
using MimeKit;

/// <summary>
/// Servicio que implementa el envio de emails a traves de STMP.
/// Justificacion: Se requiere implementar 2FA por mail. Usamos un servicio para agrupar funcionalidades de SMTP.
/// </summary>
public class EmailService
{
    private readonly IConfiguration _configuration;
    private EmailSettings _emailSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="configuration">Configuration para tomar parametros de appsettings.</param>
    public EmailService(IConfiguration configuration)
    {
        this._configuration = configuration;
        this._emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? throw new InvalidOperationException("Email settings not configured properly.");
    }

    /// <summary>
    /// Envia un email al destinatario por SMTP.
    /// </summary>
    /// <param name="recipientEmail">Email del destinatario.</param>
    /// <param name="subject">Asunto del mensaje.</param>
    /// <param name="message">Mensaje a enviar por el email.</param>
    /// <returns>Se implementa con Task, no se espera retorno.</returns>
    public async Task SendEmailAsync(string recipientEmail, string subject, string message)
    {
        using (var email = new MimeMessage())
        {
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(recipientEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = message };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ServicesResources.SendEmailError + ex.Message);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}