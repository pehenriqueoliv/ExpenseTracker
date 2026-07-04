using System.Net;
using System.Net.Http.Json;
using ExpenseTracker.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Tests;

/// <summary>
/// Integration tests that drive the real HTTP pipeline (routing, model binding,
/// [ApiController] validation, services and the global exception handler),
/// covering behavior the service-level unit tests cannot reach — most notably
/// the framework-produced 400 for a request that fails DTO validation.
/// </summary>
public class TransactionsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostTransaction_MissingType_Returns400()
    {
        // "type" is omitted: [ApiController] rejects it with a 400 before the
        // request ever reaches the service or the database.
        var payload = new { description = "Groceries", amount = 50.0m, personId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/transactions", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostTransaction_IncomeForMinor_Returns400WithRuleMessage()
    {
        // Create a minor through the API, then try to register income for them:
        // the business rule surfaces end-to-end as a 400 Problem Details.
        var personResponse = await _client.PostAsJsonAsync("/api/people", new { name = "Ana", age = 16 });
        personResponse.EnsureSuccessStatusCode();
        var person = await personResponse.Content.ReadFromJsonAsync<PersonResponse>();

        var response = await _client.PostAsJsonAsync(
            "/api/transactions",
            new { description = "Salário", amount = 100.0m, type = "Income", personId = person!.Id });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("menor", problem!.Detail);
    }
}
