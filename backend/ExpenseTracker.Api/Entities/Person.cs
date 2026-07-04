namespace ExpenseTracker.Api.Entities;

/// <summary>
/// Person domain entity. Kept as a plain object; the mapping (keys,
/// relationships, cascade delete) is centralized in AppDbContext via Fluent API.
/// </summary>
public class Person
{
    // Guid primary key, generated automatically by EF Core on insert.
    public Guid Id { get; set; }

    // Person name. Required-ness is validated on the input DTOs, not here.
    public string Name { get; set; } = string.Empty;

    // Age in years. Used by the business rule: minors can only register expenses.
    public int Age { get; set; }

    // Navigation property to the person's transactions (the "one" side of 1-N).
    // Cascade delete is configured in AppDbContext.
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
