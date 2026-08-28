using BookQuote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookQuote.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Quote> Quotes => Set<Quote>();
}