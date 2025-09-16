using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.tests.Services;

public class SitesServiceTests
{
    private DatabaseContext _db = null!;
    private ISitesService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var appSettings = Microsoft.Extensions.Options.Options.Create(new Kont.backend.Models.AppSettings());
        _db = new DatabaseContext(options, appSettings);
        _service = new SitesService(_db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public async Task Create_And_Get_Site_Works()
    {
        var toCreate = new Site { Name = "S1", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "s1@a.com" };
        var created = await _service.CreateSiteAsync(toCreate);
        Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));

        var list = await _service.GetSitesAsync();
        Assert.That(list.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateSite_ReturnsUpdated()
    {
        var created = await _service.CreateSiteAsync(new Site { Name = "S1", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "s1@a.com" });
        var updated = await _service.UpdateSiteAsync(created.Id, new Site { Name = "S2", Address = "Adr2", City = "City2", ZipCode = "75002", Country = "FR", State = "IDF", PhoneNumber = "0102030406", Email = "s2@a.com" });
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("S2"));
    }

    [Test]
    public async Task DeleteSite_ReturnsTrue()
    {
        var created = await _service.CreateSiteAsync(new Site { Name = "S1", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "s1@a.com" });
        var ok = await _service.DeleteSiteAsync(created.Id);
        Assert.That(ok, Is.True);
    }
}


