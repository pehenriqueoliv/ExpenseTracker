using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Exceptions;

/// <summary>
/// Global exception handler. Converts domain exceptions into responses in the
/// Problem Details (RFC 7807) format, keeping controllers free of error handling.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Map each exception type to an HTTP status and title.
        // The user-facing message (Detail) is kept in Portuguese to match the UI.
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            // Any other exception becomes a 500 (unexpected error).
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        httpContext.Response.StatusCode = status;

        // Build the Problem Details body using the exception message as 'detail'.
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message
            }
        });
    }
}
