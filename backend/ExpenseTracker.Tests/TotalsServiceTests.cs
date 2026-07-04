using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Tests;

public class TotalsServiceTests
{
    private static Person AddPerson(AppDbContext context, string name, int age)
    {
        var person = new Person { Name = name, Age = age };
        context.People.Add(person);
        return person;
    }

    private static void AddTransaction(AppDbContext context, Person person, decimal amount, TransactionType type)
    {
        context.Transactions.Add(new Transaction
        {
            Description = type.ToString(),
            Amount = amount,
            Type = type,
            PersonId = person.Id,
            Person = person
        });
    }

    [Fact]
    public async Task GetAsync_NoData_ReturnsZeroedTotals()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();

        var totals = await new TotalsService(context).GetAsync(CancellationToken.None);

        Assert.Empty(totals.People);
        Assert.Equal(0m, totals.OverallTotalIncome);
        Assert.Equal(0m, totals.OverallTotalExpenses);
        Assert.Equal(0m, totals.OverallBalance);
    }

    [Fact]
    public async Task GetAsync_ComputesPerPersonAndOverallTotals()
    {
        using var factory = new TestDbContextFactory();

        await using (var seed = factory.CreateContext())
        {
            var alice = AddPerson(seed, "Alice", 30);
            var bob = AddPerson(seed, "Bob", 40);

            // Alice: +1000 income, -250 expenses => balance 750.
            AddTransaction(seed, alice, 1000m, TransactionType.Income);
            AddTransaction(seed, alice, 200m, TransactionType.Expense);
            AddTransaction(seed, alice, 50m, TransactionType.Expense);

            // Bob: +500 income, -600 expenses => balance -100.
            AddTransaction(seed, bob, 500m, TransactionType.Income);
            AddTransaction(seed, bob, 600m, TransactionType.Expense);

            await seed.SaveChangesAsync();
        }

        await using var context = factory.CreateContext();
        var totals = await new TotalsService(context).GetAsync(CancellationToken.None);

        // Ordered by name: Alice first, then Bob.
        var aliceTotals = totals.People[0];
        Assert.Equal("Alice", aliceTotals.Name);
        Assert.Equal(1000m, aliceTotals.TotalIncome);
        Assert.Equal(250m, aliceTotals.TotalExpenses);
        Assert.Equal(750m, aliceTotals.Balance);

        var bobTotals = totals.People[1];
        Assert.Equal("Bob", bobTotals.Name);
        Assert.Equal(500m, bobTotals.TotalIncome);
        Assert.Equal(600m, bobTotals.TotalExpenses);
        Assert.Equal(-100m, bobTotals.Balance);

        // Overall = sum across everyone.
        Assert.Equal(1500m, totals.OverallTotalIncome);
        Assert.Equal(850m, totals.OverallTotalExpenses);
        Assert.Equal(650m, totals.OverallBalance);
    }

    [Fact]
    public async Task GetAsync_PersonWithNoTransactions_AppearsWithZeros()
    {
        using var factory = new TestDbContextFactory();

        await using (var seed = factory.CreateContext())
        {
            AddPerson(seed, "Lonely", 25);
            await seed.SaveChangesAsync();
        }

        await using var context = factory.CreateContext();
        var totals = await new TotalsService(context).GetAsync(CancellationToken.None);

        var lonely = Assert.Single(totals.People);
        Assert.Equal(0m, lonely.TotalIncome);
        Assert.Equal(0m, lonely.TotalExpenses);
        Assert.Equal(0m, lonely.Balance);
    }
}
