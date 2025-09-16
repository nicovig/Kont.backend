using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface ISitesService
{
    Task<IEnumerable<Site>> GetSitesAsync();
    Task<Site?> GetSiteAsync(Guid siteId);
    Task<Site> CreateSiteAsync(Site site);
    Task<bool> DeleteSiteAsync(Guid siteId);
    Task<Site?> UpdateSiteAsync(Guid siteId, Site site);
}

public class SitesService : ISitesService
{
    private readonly IDatabaseContext _context;

    public SitesService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Site>> GetSitesAsync()
    {
        return await _context.Site.AsNoTracking().ToListAsync();
    }

    public async Task<Site?> GetSiteAsync(Guid siteId)
    {
        return await _context.Site.AsNoTracking().FirstOrDefaultAsync(s => s.Id == siteId);
    }

    public async Task<Site> CreateSiteAsync(Site site)
    {
        site.Id = site.Id == Guid.Empty ? Guid.NewGuid() : site.Id;
        _context.Site.Add(site);
        await _context.SaveChangesAsync();
        return site;
    }

    public async Task<bool> DeleteSiteAsync(Guid siteId)
    {
        var site = await _context.Site.FindAsync(siteId);
        if (site == null)
        {
            return false;
        }
        _context.Site.Remove(site);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Site?> UpdateSiteAsync(Guid siteId, Site site)
    {
        var existing = await _context.Site.FindAsync(siteId);
        if (existing == null)
        {
            return null;
        }
        existing.Name = site.Name;
        existing.Address = site.Address;
        existing.City = site.City;
        existing.ZipCode = site.ZipCode;
        existing.Country = site.Country;
        existing.State = site.State;
        existing.PhoneNumber = site.PhoneNumber;
        existing.Email = site.Email;
        existing.Description = site.Description;
        existing.Logo = site.Logo;
        await _context.SaveChangesAsync();
        return existing;
    }
}


