using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IAdministratorsService
{
    Task<IEnumerable<Administrator>> GetAdministratorsAsync();
    Task<Administrator?> GetAdministratorAsync(Guid id);
    Task<Administrator> CreateAdministratorAsync(Administrator admin);
    Task<Administrator?> UpdateAdministratorAsync(Guid id, Administrator admin);
    Task<bool> DeleteAdministratorAsync(Guid id);
}

public class AdministratorsService : IAdministratorsService
{
    private readonly IDatabaseContext _context;

    public AdministratorsService(IDatabaseContext context)
    {
        _context = context;
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

    public async Task<Administrator?> GetAdministratorAsync(Guid id)
    {
        return await _context.Administrator
            .Include(a => a.Role)
            .Include(a => a.Subscription)
            .Include(a => a.Sites)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Administrator> CreateAdministratorAsync(Administrator admin)
    {
        admin.Id = admin.Id == Guid.Empty ? Guid.NewGuid() : admin.Id;
        _context.Administrator.Add(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task<Administrator?> UpdateAdministratorAsync(Guid id, Administrator admin)
    {
        var existing = await _context.Administrator
            .Include(a => a.Sites)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.Firstname = admin.Firstname;
        existing.Lastname = admin.Lastname;
        existing.Email = admin.Email;
        existing.Password = admin.Password;
        existing.PhoneNumber = admin.PhoneNumber;
        existing.IsActive = admin.IsActive;
        existing.Role = admin.Role;
        existing.Subscription = admin.Subscription;
        existing.Sites = admin.Sites;

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


