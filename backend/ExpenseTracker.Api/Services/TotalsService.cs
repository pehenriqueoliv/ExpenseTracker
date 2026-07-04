using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public interface ITotalsService
{
    Task<TotalsResponse> GetAsync(CancellationToken ct);
}

/// <summary>
/// Business rule 3: consolidates the totals per person and the overall totals.
/// </summary>
public class TotalsService : ITotalsService
{
    private readonly AppDbContext _db;

    public TotalsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TotalsResponse> GetAsync(CancellationToken ct)
    {
        // People and transactions are loaded and aggregated in memory on purpose:
        // SQLite stores decimal as TEXT and has no native decimal SUM, so summing
        // here avoids precision issues and SQL translation limitations. This is
        // simple and safe for the data volume of this application.
        var people = await _db.People
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        var transactions = await _db.Transactions
            .AsNoTracking()
            .ToListAsync(ct);

        // Group transactions by person for fast lookup.
        var transactionsByPerson = transactions
            .GroupBy(t => t.PersonId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var totalsPerPerson = new List<PersonTotalResponse>();

        foreach (var person in people)
        {
            // This person's transactions (empty if they have none).
            var owned = transactionsByPerson.GetValueOrDefault(person.Id, new List<Transaction>());

            // Sum income and expenses separately.
            var totalIncome = owned
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = owned
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            // Individual balance = income - expenses.
            var balance = totalIncome - totalExpenses;

            totalsPerPerson.Add(new PersonTotalResponse(
                person.Id, person.Name, totalIncome, totalExpenses, balance));
        }

        // Overall totals: sum of every person's totals.
        var overallIncome = totalsPerPerson.Sum(p => p.TotalIncome);
        var overallExpenses = totalsPerPerson.Sum(p => p.TotalExpenses);
        var overallBalance = overallIncome - overallExpenses;

        return new TotalsResponse(
            totalsPerPerson, overallIncome, overallExpenses, overallBalance);
    }
}
