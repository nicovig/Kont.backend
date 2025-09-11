using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kont.backend.DAL.DatabaseContext;

public interface IDatabaseContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    DatabaseFacade Database { get; }

    DbSet<Activity> Activity { get; }
    DbSet<ActivitySummaryData> ActivitySummaryData { get; }
    DbSet<Administrator> Administrator { get; }
    DbSet<Event> Event { get; }
    DbSet<GameSession> GameSession { get; }
    DbSet<GroupScore> GroupScore { get; }
    DbSet<Player> Player { get; }
    DbSet<PlayerActivityScore> PlayerActivityScore { get; }
    DbSet<PlayerGlobalScore> PlayerGlobalScore { get; }
    DbSet<PlayerGroup> PlayerGroup { get; }
    DbSet<PlayerRegistration> PlayerRegistration { get; }
    DbSet<PlayerScore> PlayerScore { get; }
    DbSet<Pool> Pool { get; }
    DbSet<Role> Role { get; }
    DbSet<ScoringMetric> ScoringMetric { get; }
    DbSet<Site> Site { get; }
    DbSet<Subscription> Subscription { get; }
}
