using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace Kont.backend.tests.Tools;

public abstract class DatabaseTester
{
    protected DatabaseForTests _dataHelper;
    protected DatabaseContext _context;
    protected IDbContextTransaction? _transaction;

    [SetUp]
    public async Task DatabaseSetUp()
    {
        _context = DatabaseContextForTests.Instance.CreateContext();
        _dataHelper = new DatabaseForTests(_context);

        _transaction = await _context.Database.BeginTransactionAsync();
        await _dataHelper.Empty();
    }

    [TearDown]
    public void DatabaseTearDown()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }
}