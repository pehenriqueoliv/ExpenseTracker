using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Exceptions;
using ExpenseTracker.Api.Services;

namespace ExpenseTracker.Tests;

public class PersonServiceTests
{
    [Fact]
    public async Task CreateAsync_PersistsPerson_AndReturnsGeneratedId()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var service = new PersonService(context);

        var response = await service.CreateAsync(new CreatePersonRequest("Alice", 30), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Alice", response.Name);
        Assert.Equal(30, response.Age);
    }

    [Fact]
    public async Task ListAsync_ReturnsPeopleOrderedByName()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var service = new PersonService(context);

        await service.CreateAsync(new CreatePersonRequest("Charlie", 40), CancellationToken.None);
        await service.CreateAsync(new CreatePersonRequest("Alice", 30), CancellationToken.None);
        await service.CreateAsync(new CreatePersonRequest("Bob", 20), CancellationToken.None);

        var people = await service.ListAsync(CancellationToken.None);

        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, people.Select(p => p.Name));
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsNotFound()
    {
        using var factory = new TestDbContextFactory();
        await using var context = factory.CreateContext();
        var service = new PersonService(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_RemovesPerson_AndCascadeDeletesTheirTransactions()
    {
        using var factory = new TestDbContextFactory();

        // Arrange: a person with one transaction, created in a separate context.
        Guid personId;
        await using (var seed = factory.CreateContext())
        {
            var person = new Person { Name = "Alice", Age = 30 };
            seed.People.Add(person);
            seed.Transactions.Add(new Transaction
            {
                Description = "Salary",
                Amount = 100m,
                Type = TransactionType.Income,
                PersonId = person.Id,
                Person = person
            });
            await seed.SaveChangesAsync();
            personId = person.Id;
        }

        // Act: delete the person through the service.
        await using (var act = factory.CreateContext())
        {
            await new PersonService(act).DeleteAsync(personId, CancellationToken.None);
        }

        // Assert: both the person and their transactions are gone (cascade delete).
        await using var assert = factory.CreateContext();
        Assert.Empty(assert.People);
        Assert.Empty(assert.Transactions);
    }
}
