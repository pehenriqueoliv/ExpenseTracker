using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Transactions controller: create and list only (no update/delete, by design).
/// </summary>
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// POST /api/transactions — creates a transaction.
    /// May return 404 (person not found) or 400 (minor trying to register income),
    /// both handled by the GlobalExceptionHandler in Problem Details format.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> Create(
        [FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var transaction = await _transactionService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(List), new { id = transaction.Id }, transaction);
    }

    /// <summary>GET /api/transactions — lists every transaction.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> List(CancellationToken ct)
    {
        var transactions = await _transactionService.ListAsync(ct);
        return Ok(transactions);
    }
}
