using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

/// <summary>
/// People controller. Intentionally thin: it receives the HTTP request, delegates
/// to the service (business rules) and returns the response. [ApiController]
/// enables automatic model validation, replying 400 in Problem Details whenever a
/// DTO violates its Data Annotations.
/// </summary>
[ApiController]
[Route("api/people")]
public class PeopleController : ControllerBase
{
    private readonly IPersonService _personService;

    public PeopleController(IPersonService personService)
    {
        _personService = personService;
    }

    /// <summary>POST /api/people — creates a person.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PersonResponse>> Create(
        [FromBody] CreatePersonRequest request, CancellationToken ct)
    {
        var person = await _personService.CreateAsync(request, ct);
        // 201 Created with a Location header pointing to the resource.
        return CreatedAtAction(nameof(List), new { id = person.Id }, person);
    }

    /// <summary>GET /api/people — lists every person.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PersonResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PersonResponse>>> List(CancellationToken ct)
    {
        var people = await _personService.ListAsync(ct);
        return Ok(people);
    }

    /// <summary>DELETE /api/people/{id} — deletes the person and, in cascade, their transactions.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _personService.DeleteAsync(id, ct);
        // 204 No Content: success with no response body.
        return NoContent();
    }
}
