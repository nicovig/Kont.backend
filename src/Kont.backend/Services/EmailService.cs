using MailKit.Net.Smtp;
using MimeKit;

namespace Kont.backend.Services;

public interface IEmailService
{
    Task SendAsync(EmailMessage message);
}

public class EmailMessage
{
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public IReadOnlyList<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}

public class EmailAttachment
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Content { get; set; }
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Kont", _configuration["Email:From"] ?? "noreply@kont.com"));
            email.To.Add(new MailboxAddress("", message.To));
            email.Subject = message.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = message.HtmlBody
            };

            foreach (var attachment in message.Attachments)
            {
                builder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }

            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var host = _configuration["Email:SmtpHost"] ?? "localhost";
            var port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var user = _configuration["Email:Username"] ?? string.Empty;
            var pass = _configuration["Email:Password"] ?? string.Empty;

            var useStartTls = !string.IsNullOrWhiteSpace(user) && port != 1025;
            var socket = useStartTls ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None;

            await client.ConnectAsync(host, port, socket);

            if (!string.IsNullOrWhiteSpace(user))
            {
                await client.AuthenticateAsync(user, pass);
            }
            await client.SendAsync(email);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", message.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", message.To);
            throw;
        }
    }
}
