namespace BookQuote.Api.Models;

public class User
{
    public int Id { get; set; }

    public required string Username { get; set; }

    public required string NormalizedUsername { get; set; }

    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Quote> Quotes { get; set; } = new();
}
