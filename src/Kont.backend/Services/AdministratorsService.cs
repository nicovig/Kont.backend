using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Request;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IAdministratorsService
{
    Task<IEnumerable<Administrator>> GetAdministratorsAsync();
    Task<Administrator?> GetAdministratorAsync(Guid id);
    Task<Administrator> CreateAdministratorAsync(CreateAdministratorRequest createRequest);
    Task<Administrator?> UpdateAdministratorAsync(Guid id, Administrator admin);
    Task<bool> DeleteAdministratorAsync(Guid id);
    Task<IEnumerable<Role>> GetRolesAsync();
}

public class AdministratorsService : IAdministratorsService
{
    private readonly IDatabaseContext _context;
    private readonly IPasswordService _passwordService;

    public AdministratorsService(IDatabaseContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<IEnumerable<Administrator>> GetAdministratorsAsync()
    {
        return await _context.Administrator
            .Include(a => a.Role)
            .Include(a => a.Subscription)
            .Include(a => a.Sites)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        return await _context.Role.AsNoTracking().ToListAsync();
    }

    public async Task<Administrator?> GetAdministratorAsync(Guid id)
    {
        return await _context.Administrator
            .Include(a => a.Role)
            .Include(a => a.Subscription)
            .Include(a => a.Sites)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Administrator> CreateAdministratorAsync(CreateAdministratorRequest createRequest)
    {
        // Récupérer le rôle existant
        var role = await _context.Role.FirstOrDefaultAsync(r => r.Id == createRequest.Role.Id);
        if (role == null)
        {
            throw new ArgumentException($"Role with id {createRequest.Role.Id} not found");
        }

        // Récupérer les sites existants
        var siteIds = createRequest.Sites.Select(s => s.Id).ToList();
        var sites = await _context.Site.Where(s => siteIds.Contains(s.Id)).ToListAsync();

        var admin = new Administrator
        {
            Id = Guid.NewGuid(),
            Firstname = createRequest.Firstname,
            Lastname = createRequest.Lastname,
            Email = createRequest.Email,
            Password = _passwordService.HashPassword(createRequest.Password),
            PhoneNumber = createRequest.PhoneNumber,
            Role = role,
            IsActive = createRequest.IsActive,
            Sites = sites,
            CreatedAt = DateTime.UtcNow
        };

        admin.Subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            Administrator = admin,
            SubscriptionType = createRequest.SubscriptionType,
        };

        _context.Administrator.Add(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task<Administrator?> UpdateAdministratorAsync(Guid id, Administrator admin)
    {
        var existing = await _context.Administrator
            .Include(a => a.Sites)
            .Include(a => a.Subscription)
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.Firstname = admin.Firstname;
        existing.Lastname = admin.Lastname;
        existing.Email = admin.Email;
        // Ne mettre à jour le mot de passe que s'il est fourni
        if (!string.IsNullOrEmpty(admin.Password))
        {
            existing.Password = _passwordService.HashPassword(admin.Password);
        }
        existing.PhoneNumber = admin.PhoneNumber;
        existing.IsActive = admin.IsActive;

        if (admin.Role != null)
        {
            if (admin.Role.Id != Guid.Empty)
            {
                var role = await _context.Role.FindAsync(admin.Role.Id);
                if (role != null) existing.Role = role;
            }
            else
            {
                var role = await _context.Role.FirstOrDefaultAsync(r => r.RoleType == admin.Role.RoleType);
                if (role != null) existing.Role = role;
            }
        }

        if (existing.Subscription == null)
        {
            existing.Subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Administrator = existing
            };
        }
        else if (admin.Subscription != null)
        {
            existing.Subscription.SubscriptionType = admin.Subscription.SubscriptionType;
            existing.Subscription.PaidAt = admin.Subscription.PaidAt;
            existing.Subscription.ExpiresAt = admin.Subscription.ExpiresAt;
        }

        if (admin.Sites != null)
        {
            var siteIds = admin.Sites.Select(s => s.Id).ToList();
            var sites = await _context.Site.Where(s => siteIds.Contains(s.Id)).ToListAsync();
            existing.Sites.Clear();
            foreach (var s in sites)
            {
                existing.Sites.Add(s);
            }
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAdministratorAsync(Guid id)
    {
        var admin = await _context.Administrator.FindAsync(id);
        if (admin == null)
        {
            return false;
        }
        _context.Administrator.Remove(admin);
        await _context.SaveChangesAsync();
        return true;
    }
}


