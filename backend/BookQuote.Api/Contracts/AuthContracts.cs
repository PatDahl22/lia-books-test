using System.ComponentModel.DataAnnotations;

namespace BookQuote.Api.Contracts;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Username may only contain letters, numbers, dots, underscores, and hyphens.")]
    public string Username { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    string Username);

public sealed record CurrentUserResponse(int Id, string Username);
