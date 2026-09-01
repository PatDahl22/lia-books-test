namespace BookQuote.Api.Models;

public class Quote
{
    public int Id { get; set; }

    public required string Text { get; set; }

    public required string Author { get; set; }

    public string? Source { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public User? User { get; set; }
}
