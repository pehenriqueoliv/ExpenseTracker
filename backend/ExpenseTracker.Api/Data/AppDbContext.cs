using ExpenseTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Data;

/// <summary>
/// EF Core database context: the session with the database and the entry point
/// for data access. Each DbSet&lt;T&gt; is a table that can be queried with LINQ.
/// </summary>
public class AppDbContext : DbContext
{
    // Options (provider, connection string) are supplied via dependency injection,
    // configured in Program.cs.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Database tables.
    public DbSet<Person> People => Set<Person>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // Model configuration via Fluent API, kept here to keep the entities clean.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(person =>
        {
            person.HasKey(p => p.Id);
            person.Property(p => p.Name).IsRequired().HasMaxLength(200);
            person.Property(p => p.Age).IsRequired();

            // 1-N relationship: a Person has many Transactions.
            // OnDelete(Cascade) enforces business rule 1: deleting a Person
            // automatically deletes all of their transactions at the database level.
            person.HasMany(p => p.Transactions)
                  .WithOne(t => t.Person)
                  .HasForeignKey(t => t.PersonId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transaction>(transaction =>
        {
            transaction.HasKey(t => t.Id);
            transaction.Property(t => t.Description).IsRequired().HasMaxLength(300);

            // Monetary precision. SQLite stores decimal as TEXT, preserving
            // precision and avoiding floating-point rounding errors.
            transaction.Property(t => t.Amount).HasPrecision(18, 2);

            // Store the enum as text ("Expense"/"Income") instead of a number,
            // keeping the database readable.
            transaction.Property(t => t.Type).HasConversion<string>();
        });
    }
}
