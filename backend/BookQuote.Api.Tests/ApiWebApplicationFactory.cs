using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookQuote.Api.Tests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"lia-books-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");
        builder.UseSetting("Jwt:Key", "integration-test-key-with-at-least-thirty-two-bytes");
        builder.UseSetting("Jwt:Issuer", "BookQuote.Api.Tests");
        builder.UseSetting("Jwt:Audience", "BookQuote.Api.Tests");
        builder.UseSetting("Jwt:ExpiryMinutes", "10");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
