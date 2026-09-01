using System.ComponentModel.DataAnnotations;

namespace BookQuote.Api.Contracts;

public sealed class CreateQuoteRequest
{
    [Required]
    [StringLength(4_000)]
    public string Text { get; init; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; init; } = string.Empty;

    [StringLength(200)]
    public string? Source { get; init; }
}

public sealed class UpdateQuoteRequest
{
    [Required]
    [StringLength(4_000)]
    public string Text { get; init; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; init; } = string.Empty;

    [StringLength(200)]
    public string? Source { get; init; }
}

public sealed record QuoteResponse(
    int Id,
    string Text,
    string Author,
    string? Source,
    DateTime CreatedAt,
    DateTime UpdatedAt);
