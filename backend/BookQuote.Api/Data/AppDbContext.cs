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

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(book => book.Title).HasMaxLength(200);
            entity.Property(book => book.Author).HasMaxLength(150);
            entity.Property(book => book.Description).HasMaxLength(2_000);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Username).HasMaxLength(50);
            entity.Property(user => user.NormalizedUsername).HasMaxLength(50);
            entity.HasIndex(user => user.NormalizedUsername).IsUnique();
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.Property(quote => quote.Text).HasMaxLength(4_000);
            entity.Property(quote => quote.Author).HasMaxLength(150);
            entity.Property(quote => quote.Source).HasMaxLength(200);
            entity.HasOne(quote => quote.User)
                .WithMany(user => user.Quotes)
                .HasForeignKey(quote => quote.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
