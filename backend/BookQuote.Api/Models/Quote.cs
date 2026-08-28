namespace BookQuote.Api.Models;

public class Quote
{
    public int Id { get; set; }

    public required string Text { get; set; }

    public int? Page { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int BookId { get; set; }

    public Book? Book { get; set; }
}