using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Kont.backend.Models.Request;
using Kont.backend.Models.Response;
using Kont.backend.Models;
using Microsoft.Extensions.Options;

namespace Kont.backend.Services;

public interface IEventsService
{
    Task<IEnumerable<Event>> GetEventsAsync();
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event?> GetEventByLinkAsync(string eventLink);
    Task<Event> CreateEventAsync(CreateEventRequest request, Guid createdById);
    Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest request);
    Task<bool> DeleteEventAsync(Guid id);
    Task<Event?> UpdateEventAllPlayersPresentAsync(Guid eventId, bool isAllPlayersPresent);
    Task<Event?> UpdateEventPlayerIsPresentAsync(Guid eventId, Guid playerRegistrationId, bool isPresent);    
    Task<IEnumerable<PlayerRegistrationResponse>> GetPlayerRegistrationsByEventAsync(Guid eventId);
    Task<Event?> EndEventAsync(Guid eventId);
}

public class EventsService : IEventsService
{
    private readonly IDatabaseContext _context;

    private readonly AppSettings _settings;

    public EventsService(IDatabaseContext context, IOptions<AppSettings> option)
    {
        _context = context;
        _settings = option.Value;
    }

    public async Task<IEnumerable<Event>> GetEventsAsync()
    {
        return await _context.Event
            .Include(e => e.Site)
            .Include(e => e.Activities)
            .Include(e => e.Pools)
            .Include(e => e.CreatedBy)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        return await _context.Event
            .Include(e => e.Site)
            .Include(e => e.Activities)
            .Include(e => e.Pools)
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetEventByLinkAsync(string eventLink)
    {
        return await _context.Event
            .Include(e => e.Site)
            .Include(e => e.Activities)
            .Include(e => e.Pools)
            .Include(e => e.CreatedBy)
            .FirstOrDefaultAsync(e => e.EventLink == eventLink);
    }

    public async Task<Event> CreateEventAsync(CreateEventRequest request, Guid createdById)
    {
        var site = await _context.Site.FindAsync(request.SiteId) ?? throw new ArgumentException("Site not found");
        var admin = await _context.Administrator.Include(a => a.Subscription).FirstOrDefaultAsync(a => a.Id == createdById) ?? throw new ArgumentException("Administrator not found");

        var createdEventLength = await _context.Event.CountAsync(e => e.CreatedBy.Id == createdById);

        if (admin.Subscription.SubscriptionType == SubscriptionType.Esae && createdEventLength > _settings.AccountLimit.CreationEventNumberLimitForEsaeSubscription || 
            admin.Subscription.SubscriptionType == SubscriptionType.Deraou && createdEventLength > _settings.AccountLimit.CreationEventNumberLimitForDeraouSubscription) {
            throw new ArgumentException("You have reached the maximum number of events for your subscription");
        }

        var activities = new List<Activity>();
        if (request.ActivityIds?.Count > 0)
        {
            activities = await _context.Activity.Where(a => request.ActivityIds.Contains(a.Id)).ToListAsync();
        }

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            EventLink = request.EventLink,
            StartedAt = request.StartedAt,
            EndedAt = request.EndedAt,
            Site = site,
            Status = request.Status,
            CreatedBy = admin,
            CreatedAt = DateTime.UtcNow,
            Activities = activities
        };

        _context.Event.Add(ev);
        await _context.SaveChangesAsync();

        var pool = new Pool
        {
            Id = Guid.NewGuid(),
            Name = ev.Name,
            Description = null,
            QrCode = ev.EventLink,
            Event = ev,
            StartedAt = ev.StartedAt,
            EndedAt = ev.EndedAt,
            Status = ev.Status switch
            {
                EventStatus.Pending => PoolStatus.Pending,
                EventStatus.Active => PoolStatus.Active,
                EventStatus.Completed => PoolStatus.Completed,
                EventStatus.Cancelled => PoolStatus.Cancelled,
                _ => PoolStatus.Pending
            },
            IsActive = true,
        };
        _context.Pool.Add(pool);
        await _context.SaveChangesAsync();
        return ev;
    }

    public async Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest request)
    {
        var existing = await _context.Event
            .Include(e => e.Activities)
            .Include(e => e.Pools)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (existing == null) return null;

        var activities = await _context.Activity.Where(a => request.ActivityIds.Contains(a.Id)).ToListAsync();

        existing.Name = request.Name;
        existing.EventLink = request.EventLink;
        existing.StartedAt = request.StartedAt;
        existing.EndedAt = request.EndedAt;
        existing.Status = request.Status;
        existing.Activities = activities;

        if (request.SiteId != Guid.Empty)
        {
            var site = await _context.Site.FindAsync(request.SiteId) ?? throw new ArgumentException("Site not found");
            existing.Site = site;
        }

        await _context.SaveChangesAsync();

        var pool = existing.Pools.FirstOrDefault();
        if (pool == null)
        {
            pool = new Pool
            {
                Id = Guid.NewGuid(),
                Name = existing.Name,
                Description = null,
                QrCode = existing.EventLink,
                Event = existing,
                StartedAt = existing.StartedAt,
                EndedAt = existing.EndedAt,
                Status = PoolStatus.Pending,
                IsActive = true,
            };
            _context.Pool.Add(pool);
        }
        else
        {
            pool.Name = existing.Name;
            pool.StartedAt = existing.StartedAt;
            pool.EndedAt = existing.EndedAt;
            pool.Status = existing.Status switch
            {
                EventStatus.Pending => PoolStatus.Pending,
                EventStatus.Active => PoolStatus.Active,
                EventStatus.Completed => PoolStatus.Completed,
                EventStatus.Cancelled => PoolStatus.Cancelled,
                _ => pool.Status
            };
            if (!string.IsNullOrWhiteSpace(existing.EventLink))
            {
                pool.QrCode = existing.EventLink;
            }
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        var ev = await _context.Event.FindAsync(id);
        if (ev == null) return false;
        _context.Event.Remove(ev);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Event?> UpdateEventAllPlayersPresentAsync(Guid eventId, bool isAllPlayersPresent)
    {
        var eventDb = await _context.Event.Include(e => e.Pools).ThenInclude(p => p.PlayerRegistrations).FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventDb == null) return null;
        if (eventDb.Pools.Count == 0) return null;
        if (eventDb.Pools[0].PlayerRegistrations.Count == 0) return null;
        eventDb.Pools[0].IsAllPlayersPresent = isAllPlayersPresent;
        if (isAllPlayersPresent)
        {
            var now = DateTime.UtcNow;
            foreach (var pr in eventDb.Pools[0].PlayerRegistrations)
            {
                if (!pr.CheckedInAt.HasValue)
                {
                    pr.CheckedInAt = now;
                }
            }
        }
        else
        {
            foreach (var pr in eventDb.Pools[0].PlayerRegistrations)
            {
                pr.CheckedInAt = null;
            }
        }
        await _context.SaveChangesAsync();
        return eventDb;
    }

    public async Task<Event?> UpdateEventPlayerIsPresentAsync(Guid eventId, Guid playerRegistrationId, bool isPresent)
    {
        var eventDb = await _context.Event.Include(e => e.Pools).ThenInclude(p => p.PlayerRegistrations).FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventDb == null) return null;
        if (eventDb.Pools.Count == 0) return null;
        if (eventDb.Pools[0].PlayerRegistrations.Count == 0) return null;
        var playerRegistration = eventDb.Pools[0].PlayerRegistrations.FirstOrDefault(pr => pr.Id == playerRegistrationId);
        if (playerRegistration == null) return null;
        playerRegistration.CheckedInAt = isPresent ? DateTime.UtcNow : null;       
        await _context.SaveChangesAsync();
        return eventDb;
    }

    public async Task<IEnumerable<PlayerRegistrationResponse>> GetPlayerRegistrationsByEventAsync(Guid eventId)
    {
        var eventEntity = await _context.Event
            .Include(e => e.Pools)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            throw new ArgumentException("Event not found");
        }

        var pool = eventEntity.Pools.FirstOrDefault();
        if (pool == null)
        {
            throw new ArgumentException("No pool found for this event");
        }

        var registrations = await _context.PlayerRegistration
            .Include(pr => pr.Player)
            .Where(pr => pr.Pool.Id == pool.Id)
            .OrderBy(pr => pr.RegisteredAt)
            .ToListAsync();

        return registrations.Select(pr => new PlayerRegistrationResponse
        {
            Id = pr.Id,
            PlayerFirstname = pr.Player.Firstname,
            PlayerLastname = pr.Player.Lastname,
            PlayerEmail = pr.Player.Email,
            PlayerUsername = pr.Player.Username,
            PlayerType = pr.PlayerType,
            RegisteredAt = pr.RegisteredAt,
            CheckedInAt = pr.CheckedInAt
        });
    }

    public async Task<Event?> EndEventAsync(Guid eventId)
    {
        var eventDb = await _context.Event
            .Include(e => e.Pools)
            .FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventDb == null) return null;

        var hasActiveSession = await _context.GameSession.AnyAsync(gs => gs.Pool.Event.Id == eventId && gs.Status == GameSessionStatus.Active);
        if (hasActiveSession) throw new InvalidOperationException("Event cannot be completed while a game session is active");

        eventDb.Status = EventStatus.Completed;
        if (!eventDb.EndedAt.HasValue) eventDb.EndedAt = DateTime.UtcNow;
        foreach (var pool in eventDb.Pools)
        {
            pool.Status = PoolStatus.Completed;
            if (!pool.EndedAt.HasValue) pool.EndedAt = eventDb.EndedAt;
        }
        await _context.SaveChangesAsync();
        return eventDb;
    }
}


