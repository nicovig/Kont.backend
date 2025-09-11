using Kont.backend.DAL;
using Kont.backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace Kont.backend.DAL.DatabaseContext;

public class DatabaseContext : DbContext, IDatabaseContext
{
    private readonly AppSettings _settings;

    public DatabaseContext(DbContextOptions<DatabaseContext> options, IOptions<AppSettings> option)
   : base(options)
    {
        _settings = option.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSnakeCaseNamingConvention();

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif
    }
    public DbSet<Activity> Activity { get; set; } = null!;
    public DbSet<ActivitySummaryData> ActivitySummaryData { get; set; } = null!;
    public DbSet<Administrator> Administrator { get; set; } = null!;
    public DbSet<Event> Event { get; set; } = null!;
    public DbSet<GameSession> GameSession { get; set; } = null!;
    public DbSet<GroupScore> GroupScore { get; set; } = null!;
    public DbSet<Player> Player { get; set; } = null!;
    public DbSet<PlayerActivityScore> PlayerActivityScore { get; set; } = null!;
    public DbSet<PlayerGlobalScore> PlayerGlobalScore { get; set; } = null!;
    public DbSet<PlayerGroup> PlayerGroup { get; set; } = null!;
    public DbSet<PlayerRegistration> PlayerRegistration { get; set; } = null!;
    public DbSet<PlayerScore> PlayerScore { get; set; } = null!;
    public DbSet<Pool> Pool { get; set; } = null!;
    public DbSet<Role> Role { get; set; } = null!;
    public DbSet<ScoringMetric> ScoringMetric { get; set; } = null!;
    public DbSet<Site> Site { get; set; } = null!;
    public DbSet<Subscription> Subscription { get; set; } = null!;    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .Property(e => e.RoleType)
            .HasConversion(new EnumToStringConverter<RoleType>());

        modelBuilder.Entity<Subscription>()
            .Property(e => e.SubscriptionType)
            .HasConversion(new EnumToStringConverter<SubscriptionType>());

        modelBuilder.Entity<Player>()
            .Property(e => e.PlayerType)
            .HasConversion(new EnumToStringConverter<PlayerType>());


        modelBuilder.Entity<GameSession>()
            .Property(e => e.Status)
            .HasConversion(new EnumToStringConverter<GameSessionStatus>());

        modelBuilder.Entity<Pool>()
            .Property(e => e.Status)
            .HasConversion(new EnumToStringConverter<PoolStatus>());

        modelBuilder.Entity<Event>()
            .Property(e => e.Status)
            .HasConversion(new EnumToStringConverter<EventStatus>());
    }
}