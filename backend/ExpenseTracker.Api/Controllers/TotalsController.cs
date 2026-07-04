using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// Totals controller (business rule 3): consolidates income, expenses and balance
/// per person plus the overall totals.
/// </summary>
[ApiController]
[Route("api/totals")]
public class TotalsController : ControllerBase
{
    private readonly ITotalsService _totalsService;

    public TotalsController(ITotalsService totalsService)
    {
        _totalsService = totalsService;
    }

    /// <summary>GET /api/totals — returns the per-person totals and the overall totals.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TotalsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TotalsResponse>> Get(CancellationToken ct)
    {
        var totals = await _totalsService.GetAsync(ct);
        return Ok(totals);
    }
}
