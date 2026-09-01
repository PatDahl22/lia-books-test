using System.Security.Claims;
using BookQuote.Api.Contracts;
using BookQuote.Api.Data;
using BookQuote.Api.Models;
using BookQuote.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookQuote.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private static readonly (string Text, string Author, string? Source)[] StarterQuotes =
    [
        ("The only way to do great work is to love what you do.", "Steve Jobs", null),
        ("It always seems impossible until it's done.", "Nelson Mandela", null),
        ("Do what you can, with what you have, where you are.", "Theodore Roosevelt", null),
        ("Success is not final, failure is not fatal: it is the courage to continue that counts.", "Winston Churchill", null),
        ("The future depends on what you do today.", "Mahatma Gandhi", null)
    ];

    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var normalizedUsername = NormalizeUsername(username);

        if (await _dbContext.Users.AnyAsync(
                user => user.NormalizedUsername == normalizedUsername,
                cancellationToken))
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Username is already registered."
            });
        }

        var user = new User
        {
            Username = username,
            NormalizedUsername = normalizedUsername,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        foreach (var starterQuote in StarterQuotes)
        {
            user.Quotes.Add(new Quote
            {
                Text = starterQuote.Text,
                Author = starterQuote.Author,
                Source = starterQuote.Source
            });
        }

        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Username is already registered."
            });
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return CreatedAtAction(
            nameof(GetCurrentUser),
            new AuthResponse(token, expiresAt, user.Username));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedUsername = NormalizeUsername(request.Username);
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            item => item.NormalizedUsername == normalizedUsername,
            cancellationToken);

        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid username or password."
            });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid username or password."
            });
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, expiresAt, user.Username));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse(userId, User.Identity?.Name ?? string.Empty));
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim().ToUpperInvariant();
    }
}
