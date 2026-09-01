using System.ComponentModel.DataAnnotations;

namespace BookQuote.Api.Contracts;

public sealed class CreateQuoteRequest
{
    [Required]
    [StringLength(4_000)]
    public string Text { get; init; } = string.Empty;

    [Range(1, 100_000)]
    public int? Page { get; init; }

    [StringLength(2_000)]
    public string? Note { get; init; }
}

public sealed class UpdateQuoteRequest
{
    [Required]
    [StringLength(4_000)]
    public string Text { get; init; } = string.Empty;

    [Range(1, 100_000)]
    public int? Page { get; init; }

    [StringLength(2_000)]
    public string? Note { get; init; }
}

public sealed record QuoteResponse(
    int Id,
    string Text,
    int? Page,
    string? Note,
    int BookId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
