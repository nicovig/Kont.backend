using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface ISubscriptionsService
{
    Task<IEnumerable<Subscription>> GetSubscriptionsAsync();
    Task<Subscription?> GetSubscriptionAsync(Guid id);
    Task<Subscription> CreateSubscriptionAsync(Subscription sub);
    Task<Subscription?> UpdateSubscriptionAsync(Guid id, Subscription sub);
    Task<bool> DeleteSubscriptionAsync(Guid id);
}

public class SubscriptionsService : ISubscriptionsService
{
    private readonly IDatabaseContext _context;

    public SubscriptionsService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsAsync()
    {
        return await _context.Subscription.AsNoTracking().ToListAsync();
    }

    public async Task<Subscription?> GetSubscriptionAsync(Guid id)
    {
        return await _context.Subscription.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subscription> CreateSubscriptionAsync(Subscription sub)
    {
        sub.Id = sub.Id == Guid.Empty ? Guid.NewGuid() : sub.Id;
        _context.Subscription.Add(sub);
        await _context.SaveChangesAsync();
        return sub;
    }

    public async Task<Subscription?> UpdateSubscriptionAsync(Guid id, Subscription sub)
    {
        var existing = await _context.Subscription.FindAsync(id);
        if (existing == null) return null;
        existing.SubscriptionType = sub.SubscriptionType;
        existing.PaidAt = sub.PaidAt;
        existing.ExpiresAt = sub.ExpiresAt;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteSubscriptionAsync(Guid id)
    {
        var existing = await _context.Subscription.FindAsync(id);
        if (existing == null) return false;
        _context.Subscription.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}


