using System.Security.Claims;
using BookQuote.Api.Contracts;
using BookQuote.Api.Data;
using BookQuote.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookQuote.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/quotes")]
public sealed class QuotesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public QuotesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<QuoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<QuoteResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quotes = await _dbContext.Quotes
            .AsNoTracking()
            .Where(quote => quote.UserId == userId)
            .OrderByDescending(quote => quote.UpdatedAt)
            .Select(quote => new QuoteResponse(
                quote.Id,
                quote.Text,
                quote.Author,
                quote.Source,
                quote.CreatedAt,
                quote.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(quotes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quote = await _dbContext.Quotes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.UserId == userId && item.Id == id,
                cancellationToken);

        return quote is null ? NotFound() : Ok(ToResponse(quote));
    }

    [HttpPost]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuoteResponse>> Create(
        CreateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var text = request.Text.Trim();
        var author = request.Author.Trim();
        if (text.Length == 0 || author.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Text), "Quote text and author cannot be blank.");
            return ValidationProblem(ModelState);
        }

        var quote = new Quote
        {
            Text = text,
            Author = author,
            Source = NormalizeOptionalText(request.Source),
            UserId = userId
        };

        _dbContext.Quotes.Add(quote);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = quote.Id }, ToResponse(quote));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> Update(
        int id,
        UpdateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quote = await _dbContext.Quotes.SingleOrDefaultAsync(
            item => item.UserId == userId && item.Id == id,
            cancellationToken);

        if (quote is null)
        {
            return NotFound();
        }

        var text = request.Text.Trim();
        var author = request.Author.Trim();
        if (text.Length == 0 || author.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Text), "Quote text and author cannot be blank.");
            return ValidationProblem(ModelState);
        }

        quote.Text = text;
        quote.Author = author;
        quote.Source = NormalizeOptionalText(request.Source);
        quote.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(quote));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var quote = await _dbContext.Quotes.SingleOrDefaultAsync(
            item => item.UserId == userId && item.Id == id,
            cancellationToken);

        if (quote is null)
        {
            return NotFound();
        }

        _dbContext.Quotes.Remove(quote);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    private static QuoteResponse ToResponse(Quote quote)
    {
        return new QuoteResponse(
            quote.Id,
            quote.Text,
            quote.Author,
            quote.Source,
            quote.CreatedAt,
            quote.UpdatedAt);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
