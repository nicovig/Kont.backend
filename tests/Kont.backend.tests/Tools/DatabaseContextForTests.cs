using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kont.backend.tests.Tools;

// From : https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#inmemory-provider
// And : https://github.com/dotnet/EntityFramework.Docs/blob/main/samples/core/Testing/TestingWithoutTheDatabase/SqliteInMemoryBloggingControllerTest.cs
public class DatabaseContextForTests : IDisposable
{
    private static DatabaseContextForTests? _instance;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DatabaseContext> _contextOptions;
    private readonly IOptions<AppSettings> _appOptions;

    private DatabaseContextForTests()
    {
        // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
        // at the end of the test (see Dispose below).
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        // These options will be used by the context instances in this test suite, including the connection opened above.
        _contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(_connection)
            .Options;


        AppSettings settings = new() { };
        _appOptions = Options.Create(settings);
        // Create the schema and seed some data
        using var context = new DatabaseContext(_contextOptions, _appOptions);

        context.Database.Migrate();

        context.SaveChanges();

        // here if data need initialization
    }

    public static DatabaseContextForTests Instance
    {
        get
        {
            _instance ??= new DatabaseContextForTests();
            return _instance;
        }
    }

    public DatabaseContext CreateContext() => new DatabaseContext(_contextOptions, _appOptions);

    public void Dispose() => _connection.Dispose();
}
