using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Exceptions;

/// <summary>
/// Global exception handler. Converts domain exceptions into responses in the
/// Problem Details (RFC 7807) format, keeping controllers free of error handling.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    // Generic detail returned for unexpected errors, so internal exception
    // messages are never leaked to the client.
    private const string UnexpectedErrorDetail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";

    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
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

        // Domain exceptions carry safe, user-facing messages; anything else is
        // unexpected, so its message is hidden behind a generic detail and the
        // real exception is logged for diagnosis.
        var isDomainException = exception is NotFoundException or BusinessRuleException;
        var detail = isDomainException ? exception.Message : UnexpectedErrorDetail;

        if (!isDomainException)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;

        // Build the Problem Details body.
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}
