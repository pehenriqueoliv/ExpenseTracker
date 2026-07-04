namespace ExpenseTracker.Api.Entities;

/// <summary>
/// Type of a financial transaction. Serialized as text ("Expense"/"Income")
/// both in the JSON payloads and in the SQLite database for readability.
/// </summary>
public enum TransactionType
{
    Expense = 0,
    Income = 1
}
