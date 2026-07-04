using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Entities;

namespace ExpenseTracker.Api.Dtos;

/// <summary>
/// Input DTO used to create a Transaction. Format validation (required fields,
/// ranges) lives here via Data Annotations; business rules (person must exist,
/// minors can only register expenses) live in the service.
/// Validation messages are in Portuguese to match the UI.
/// </summary>
public record CreateTransactionRequest(
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(300, MinimumLength = 1, ErrorMessage = "A descrição deve ter entre 1 e 300 caracteres.")]
    string Description,

    // Amount must be positive. Uses decimal for financial precision.
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    decimal Amount,

    [Required(ErrorMessage = "O tipo é obrigatório (Expense ou Income).")]
    TransactionType Type,

    [Required(ErrorMessage = "O PersonId é obrigatório.")]
    Guid PersonId
);

/// <summary>
/// Output DTO representing a Transaction returned by the API.
/// </summary>
public record TransactionResponse(
    Guid Id,
    string Description,
    decimal Amount,
    TransactionType Type,
    Guid PersonId
);
