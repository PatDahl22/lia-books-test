using BookQuote.Api.Contracts;
using BookQuote.Api.Data;
using BookQuote.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookQuote.Api.Controllers;

[ApiController]
[Route("api/books/{bookId:int}/quotes")]
public sealed class QuotesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public QuotesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<QuoteResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<QuoteResponse>>> GetAll(
        int bookId,
        CancellationToken cancellationToken)
    {
        if (!await BookExists(bookId, cancellationToken))
        {
            return NotFound();
        }

        var quotes = await _dbContext.Quotes
            .AsNoTracking()
            .Where(quote => quote.BookId == bookId)
            .OrderBy(quote => quote.Page ?? int.MaxValue)
            .ThenBy(quote => quote.Id)
            .Select(quote => new QuoteResponse(
                quote.Id,
                quote.Text,
                quote.Page,
                quote.Note,
                quote.BookId,
                quote.CreatedAt,
                quote.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(quotes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> GetById(
        int bookId,
        int id,
        CancellationToken cancellationToken)
    {
        var quote = await _dbContext.Quotes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.BookId == bookId && item.Id == id,
                cancellationToken);

        return quote is null ? NotFound() : Ok(ToResponse(quote));
    }

    [HttpPost]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> Create(
        int bookId,
        CreateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        if (!await BookExists(bookId, cancellationToken))
        {
            return NotFound();
        }

        var text = request.Text.Trim();
        if (text.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Text), "Quote text cannot be blank.");
            return ValidationProblem(ModelState);
        }

        var quote = new Quote
        {
            Text = text,
            Page = request.Page,
            Note = NormalizeOptionalText(request.Note),
            BookId = bookId
        };

        _dbContext.Quotes.Add(quote);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { bookId, id = quote.Id },
            ToResponse(quote));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteResponse>> Update(
        int bookId,
        int id,
        UpdateQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var quote = await _dbContext.Quotes.SingleOrDefaultAsync(
            item => item.BookId == bookId && item.Id == id,
            cancellationToken);

        if (quote is null)
        {
            return NotFound();
        }

        var text = request.Text.Trim();
        if (text.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Text), "Quote text cannot be blank.");
            return ValidationProblem(ModelState);
        }

        quote.Text = text;
        quote.Page = request.Page;
        quote.Note = NormalizeOptionalText(request.Note);
        quote.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(quote));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int bookId,
        int id,
        CancellationToken cancellationToken)
    {
        var quote = await _dbContext.Quotes.SingleOrDefaultAsync(
            item => item.BookId == bookId && item.Id == id,
            cancellationToken);

        if (quote is null)
        {
            return NotFound();
        }

        _dbContext.Quotes.Remove(quote);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    internal static QuoteResponse ToResponse(Quote quote)
    {
        return new QuoteResponse(
            quote.Id,
            quote.Text,
            quote.Page,
            quote.Note,
            quote.BookId,
            quote.CreatedAt,
            quote.UpdatedAt);
    }

    private Task<bool> BookExists(int bookId, CancellationToken cancellationToken)
    {
        return _dbContext.Books.AnyAsync(book => book.Id == bookId, cancellationToken);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
