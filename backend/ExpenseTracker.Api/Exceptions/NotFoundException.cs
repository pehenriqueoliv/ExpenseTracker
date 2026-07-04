namespace ExpenseTracker.Api.Exceptions;

/// <summary>
/// Thrown when a referenced resource does not exist (e.g. a non-existent PersonId).
/// Translated to HTTP 404 by the GlobalExceptionHandler.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
