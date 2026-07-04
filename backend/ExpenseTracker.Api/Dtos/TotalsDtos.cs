namespace ExpenseTracker.Api.Dtos;

/// <summary>
/// Consolidated totals for a single person: income, expenses and balance
/// (income - expenses).
/// </summary>
public record PersonTotalResponse(
    Guid PersonId,
    string Name,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance
);

/// <summary>
/// Full response of the /api/totals endpoint: the per-person list plus the
/// overall totals across everyone.
/// </summary>
public record TotalsResponse(
    IReadOnlyList<PersonTotalResponse> People,
    decimal OverallTotalIncome,
    decimal OverallTotalExpenses,
    decimal OverallBalance
);
