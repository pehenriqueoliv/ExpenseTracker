using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Api.Dtos;

/// <summary>
/// Input DTO used to create a Person. Data Annotations provide declarative input
/// validation. On .NET 10 the attributes are placed directly on the record's
/// constructor parameters (the target the validator inspects).
/// Validation messages are in Portuguese to match the UI.
/// </summary>
public record CreatePersonRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "O nome deve ter entre 1 e 200 caracteres.")]
    string Name,

    [Range(0, 130, ErrorMessage = "A idade deve estar entre 0 e 130.")]
    int Age
);

/// <summary>
/// Output DTO representing a Person returned by the API.
/// </summary>
public record PersonResponse(
    Guid Id,
    string Name,
    int Age
);
