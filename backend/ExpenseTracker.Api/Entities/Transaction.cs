namespace ExpenseTracker.Api.Entities;

/// <summary>
/// Transaction domain entity. Represents an income or expense that belongs to a
/// Person (the "many" side of the 1-N relationship).
/// </summary>
public class Transaction
{
    // Guid primary key, generated automatically by EF Core on insert.
    public Guid Id { get; set; }

    // Free-text description of the transaction (e.g. "Salary", "Groceries").
    public string Description { get; set; } = string.Empty;

    // Monetary amount. Uses 'decimal' (not double) for financial precision.
    public decimal Amount { get; set; }

    // Type: Expense or Income.
    public TransactionType Type { get; set; }

    // Foreign key to the owning Person.
    public Guid PersonId { get; set; }

    // Navigation property to the owning Person (the "many-to-one" side).
    // Nullable because the Person is not always loaded alongside the transaction.
    public Person? Person { get; set; }
}
