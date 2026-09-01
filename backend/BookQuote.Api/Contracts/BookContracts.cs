using System.ComponentModel.DataAnnotations;

namespace BookQuote.Api.Contracts;

public sealed class CreateBookRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; init; } = string.Empty;

    [StringLength(2_000)]
    public string? Description { get; init; }

    [Range(1, 9_999)]
    public int? PublishedYear { get; init; }
}

public sealed class UpdateBookRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Author { get; init; } = string.Empty;

    [StringLength(2_000)]
    public string? Description { get; init; }

    [Range(1, 9_999)]
    public int? PublishedYear { get; init; }
}

public sealed record BookResponse(
    int Id,
    string Title,
    string Author,
    string? Description,
    int? PublishedYear,
    DateTime CreatedAt,
    DateTime UpdatedAt);
