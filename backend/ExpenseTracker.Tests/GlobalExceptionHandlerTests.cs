using ExpenseTracker.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExpenseTracker.Tests;

public class GlobalExceptionHandlerTests
{
    // Captures the ProblemDetails passed to the service so the produced 'detail'
    // and status can be asserted without a real HTTP response.
    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetails? Captured { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Captured = context.ProblemDetails;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Captured = context.ProblemDetails;
            return ValueTask.FromResult(true);
        }
    }

    private static async Task<ProblemDetails> HandleAsync(Exception exception)
    {
        var service = new CapturingProblemDetailsService();
        var handler = new GlobalExceptionHandler(service, NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.NotNull(service.Captured);
        return service.Captured!;
    }

    [Fact]
    public async Task UnexpectedException_HidesMessage_AndReturns500()
    {
        var problem = await HandleAsync(new InvalidOperationException("secret database connection string"));

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.DoesNotContain("secret", problem.Detail);
    }

    [Fact]
    public async Task NotFoundException_ExposesDomainMessage_AndReturns404()
    {
        var problem = await HandleAsync(new NotFoundException("Pessoa não encontrada."));

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Pessoa não encontrada.", problem.Detail);
    }

    [Fact]
    public async Task BusinessRuleException_ExposesDomainMessage_AndReturns400()
    {
        var problem = await HandleAsync(new BusinessRuleException("Menor de idade só pode cadastrar Despesa."));

        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Menor de idade só pode cadastrar Despesa.", problem.Detail);
    }
}
