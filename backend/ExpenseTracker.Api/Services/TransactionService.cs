using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Entities;
using ExpenseTracker.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Services;

public interface ITransactionService
{
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, CancellationToken ct);
    Task<IReadOnlyList<TransactionResponse>> ListAsync(CancellationToken ct);
}

/// <summary>
/// Transaction business rules: create and list only (no update/delete).
/// </summary>
public class TransactionService : ITransactionService
{
    // Minimum age required to register income. Minors can only register expenses.
    private const int MinimumAgeForIncome = 18;

    private readonly AppDbContext _db;

    public TransactionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, CancellationToken ct)
    {
        // Business rule 2a: the given PersonId must exist. Look up the owner.
        // If it does not exist, throw NotFoundException -> HTTP 404.
        var person = await _db.People.FirstOrDefaultAsync(p => p.Id == request.PersonId, ct);
        if (person is null)
        {
            throw new NotFoundException(
                $"Pessoa com Id '{request.PersonId}' não encontrada. Não é possível criar a transação.");
        }

        // The type is required and must be a defined enum value. [Required] on the
        // DTO rejects a missing type, but a raw out-of-range number could still
        // deserialize into an undefined value, so guard against it here as well.
        if (request.Type is not { } type || !Enum.IsDefined(type))
        {
            throw new BusinessRuleException("O tipo da transação é inválido. Use 'Expense' ou 'Income'.");
        }

        // Business rule 2b: if the person is under 18, only expenses are allowed.
        // Registering an income for a minor violates the rule -> HTTP 400.
        if (person.Age < MinimumAgeForIncome && type == TransactionType.Income)
        {
            throw new BusinessRuleException(
                $"A pessoa '{person.Name}' é menor de {MinimumAgeForIncome} anos e só pode cadastrar Despesa.");
        }

        var transaction = new Transaction
        {
            Description = request.Description,
            Amount = request.Amount,
            Type = type,
            PersonId = request.PersonId
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(ct);

        return new TransactionResponse(
            transaction.Id, transaction.Description, transaction.Amount, transaction.Type, transaction.PersonId);
    }

    public async Task<IReadOnlyList<TransactionResponse>> ListAsync(CancellationToken ct)
    {
        return await _db.Transactions
            .AsNoTracking()
            .Select(t => new TransactionResponse(t.Id, t.Description, t.Amount, t.Type, t.PersonId))
            .ToListAsync(ct);
    }
}
