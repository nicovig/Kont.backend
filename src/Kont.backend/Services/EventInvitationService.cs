using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Request;
using Kont.backend.Models.Email;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IEventInvitationService
{
    Task SendQRCodeToEmailListAsync(Guid eventId, List<string> emails, Guid senderUserId);
}

public class EventInvitationService : IEventInvitationService
{
    private readonly IDatabaseContext _context;
    private readonly IQrCodeService _qrCodeService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventInvitationService> _logger;

    public EventInvitationService(
        IDatabaseContext context,
        IQrCodeService qrCodeService,
        IEmailService emailService,
        IEmailTemplateService emailTemplateService,
        IConfiguration configuration,
        ILogger<EventInvitationService> logger)
    {
        _context = context;
        _qrCodeService = qrCodeService;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendQRCodeToEmailListAsync(Guid eventId, List<string> emails, Guid senderUserId)
    {
        var eventEntity = await _context.Event
            .Include(e => e.Pools)
            .Include(e => e.Site)
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            throw new ArgumentException("Event not found");
        }

        var pool = eventEntity.Pools.FirstOrDefault();
        if (pool == null)
        {
            throw new ArgumentException("No pool found in this event");
        }

        var baseUrl = _configuration["ApplicationBaseUrl"] ?? "https://kont.com";
        var joinUrl = $"{baseUrl}/event/{eventEntity.EventLink}/{pool.Id}";

        var qrCodeBase64 = _qrCodeService.GenerateBase64Png(joinUrl);

        var emailTasks = emails.Select(email => SendQRCodeEmailAsync(
            email, 
            eventEntity, 
            pool, 
            joinUrl, 
            qrCodeBase64, 
            "fr")); // Default to French for now

        await Task.WhenAll(emailTasks);

        _logger.LogInformation("QR codes sent to {EmailCount} recipients for event {EventId}, pool {PoolId}", 
            emails.Count, eventId, pool.Id);
    }

    private async Task SendQRCodeEmailAsync(string recipientEmail, Event eventEntity, Pool pool, string joinUrl, string qrCodeBase64, string locale)
    {
        var subject = locale == "en" 
            ? $"Join {eventEntity.Name} - Pool {pool.Name}"
            : $"Rejoindre {eventEntity.Name} - Groupe {pool.Name}";

        var emailModel = new EventInvitationEmailModel
        {
            EventName = eventEntity.Name,
            PoolName = pool.Name,
            SiteName = eventEntity.Site.Name,
            EventDate = eventEntity.StartedAt ?? DateTime.UtcNow,
            JoinUrl = joinUrl,
            QrCodeBase64 = qrCodeBase64,
            Locale = locale
        };

        var htmlBody = await _emailTemplateService.RenderEventInvitationTemplateAsync(emailModel);

        var message = new EmailMessage
        {
            To = recipientEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };

        await _emailService.SendAsync(message);
    }

}
