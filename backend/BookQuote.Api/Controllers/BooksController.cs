using BookQuote.Api.Contracts;
using BookQuote.Api.Data;
using BookQuote.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookQuote.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/books")]
public sealed class BooksController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public BooksController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BookResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var books = await _dbContext.Books
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .ThenBy(book => book.Author)
            .Select(book => new BookResponse(
                book.Id,
                book.Title,
                book.Author,
                book.Description,
                book.PublishedYear,
                book.CreatedAt,
                book.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return book is null ? NotFound() : Ok(ToResponse(book));
    }

    [HttpPost]
    [ProducesResponseType<BookResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookResponse>> Create(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        var author = request.Author.Trim();
        if (title.Length == 0 || author.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Title), "Title and author cannot be blank.");
            return ValidationProblem(ModelState);
        }

        var book = new Book
        {
            Title = title,
            Author = author,
            Description = NormalizeOptionalText(request.Description),
            PublishedYear = request.PublishedYear
        };

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = book.Id }, ToResponse(book));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<BookResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> Update(
        int id,
        UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        var title = request.Title.Trim();
        var author = request.Author.Trim();
        if (title.Length == 0 || author.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Title), "Title and author cannot be blank.");
            return ValidationProblem(ModelState);
        }

        book.Title = title;
        book.Author = author;
        book.Description = NormalizeOptionalText(request.Description);
        book.PublishedYear = request.PublishedYear;
        book.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(book));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books.FindAsync([id], cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static BookResponse ToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Description,
            book.PublishedYear,
            book.CreatedAt,
            book.UpdatedAt);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
