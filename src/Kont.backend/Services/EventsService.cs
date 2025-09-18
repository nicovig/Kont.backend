using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Kont.backend.Models.Request;
using Microsoft.Extensions.Logging;

namespace Kont.backend.Services;

public interface IEventsService
{
    Task<IEnumerable<Event>> GetEventsAsync();
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event> CreateEventAsync(CreateEventRequest request, Guid createdById);
    Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest request);
    Task<bool> DeleteEventAsync(Guid id);
    Task<Event?> UpdateEventAllPlayersPresentAsync(Guid eventId, bool isAllPlayersPresent);
}

public class EventsService : IEventsService
{
    private readonly IDatabaseContext _context;

    public EventsService(IDatabaseContext context)
    {
        _context = context;
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

    public async Task<Event> CreateEventAsync(CreateEventRequest request, Guid createdById)
    {
        var site = await _context.Site.FindAsync(request.SiteId) ?? throw new ArgumentException("Site not found");
        var admin = await _context.Administrator.FindAsync(createdById) ?? throw new ArgumentException("Administrator not found");

        var createdEventLength = await _context.Event.CountAsync(e => e.CreatedBy.Id == createdById);

        if (admin.Subscription.SubscriptionType == SubscriptionType.Esae && createdEventLength > 0 || 
            admin.Subscription.SubscriptionType == SubscriptionType.Deraou && createdEventLength > 3) {
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
            Status = PoolStatus.Pending,
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
        var eventDb = await _context.Event.Include(e => e.Pools).FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventDb == null) return null;
        if (eventDb.Pools.Count == 0) return null;
        eventDb.Pools[0].IsAllPlayersPresent = isAllPlayersPresent;
        await _context.SaveChangesAsync();
        return eventDb;
    }
}


