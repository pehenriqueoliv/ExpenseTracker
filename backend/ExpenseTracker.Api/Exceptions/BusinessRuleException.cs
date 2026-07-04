namespace ExpenseTracker.Api.Exceptions;

/// <summary>
/// Thrown when a business rule is violated (e.g. a minor trying to register an
/// income). Translated to HTTP 400 by the GlobalExceptionHandler.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
