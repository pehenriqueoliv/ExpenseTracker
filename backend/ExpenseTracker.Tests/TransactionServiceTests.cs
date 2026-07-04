using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Tests;

public class TransactionServiceTests
{
    private static async Task<Guid> SeedPersonAsync(AppDbContext context, int age)
    {
        var person = new Person { Name = "Test Person", Age = age };
        context.People.Add(person);
        await context.SaveChangesAsync();
        return person.Id;
    }

    [Fact]
    public async Task CreateAsync_UnknownPerson_ThrowsNotFound()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var service = new TransactionService(context);

        var request = new CreateTransactionRequest("Groceries", 50m, TransactionType.Expense, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_IncomeForMinor_ThrowsBusinessRule()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var personId = await SeedPersonAsync(context, age: 17);
        var service = new TransactionService(context);

        var request = new CreateTransactionRequest("Salary", 100m, TransactionType.Income, personId);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ExpenseForMinor_IsAllowed()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var personId = await SeedPersonAsync(context, age: 17);
        var service = new TransactionService(context);

        var request = new CreateTransactionRequest("Snack", 5m, TransactionType.Expense, personId);
        var response = await service.CreateAsync(request, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(TransactionType.Expense, response.Type);
    }

    [Fact]
    public async Task CreateAsync_IncomeForAdult_IsAllowed()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var personId = await SeedPersonAsync(context, age: 18);
        var service = new TransactionService(context);

        var request = new CreateTransactionRequest("Salary", 100m, TransactionType.Income, personId);
        var response = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(100m, response.Amount);
        Assert.Equal(TransactionType.Income, response.Type);
        Assert.Equal(personId, response.PersonId);
    }
}
