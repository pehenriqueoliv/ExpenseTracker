using ExpenseTracker.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

/// <summary>
/// Builds an <see cref="AppDbContext"/> backed by an in-memory SQLite database.
/// A real SQLite provider (rather than the EF Core in-memory provider) is used so
/// that relational behavior such as foreign keys and cascade delete is exercised
/// exactly as in production. The connection is kept open for the lifetime of the
/// context because an in-memory SQLite database is discarded when its last
/// connection closes.
/// </summary>
public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose() => _connection.Dispose();
}
