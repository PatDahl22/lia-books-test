using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookQuote.Api.Contracts;
using Xunit;

namespace BookQuote.Api.Tests;

public sealed class ApiFlowTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ApiFlowTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Books_require_authentication()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_returns_token_and_five_starter_quotes()
    {
        using var client = _factory.CreateClient();
        var auth = await Register(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var quoteResponse = await client.GetAsync("/api/quotes");
        await AssertSuccess(quoteResponse);
        var quotes = await quoteResponse.Content.ReadFromJsonAsync<List<QuoteResponse>>();

        Assert.NotNull(quotes);
        Assert.Equal(5, quotes.Count);
    }

    [Fact]
    public async Task Authenticated_user_can_create_update_and_delete_a_book()
    {
        using var client = _factory.CreateClient();
        var auth = await Register(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var createResponse = await client.PostAsJsonAsync("/api/books", new CreateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            PublishedYear = 2008
        });
        await AssertSuccess(createResponse);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createdBook = await createResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(createdBook);

        var updateResponse = await client.PutAsJsonAsync($"/api/books/{createdBook.Id}", new UpdateBookRequest
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Description = "A handbook of agile software craftsmanship.",
            PublishedYear = 2008
        });
        updateResponse.EnsureSuccessStatusCode();
        var updatedBook = await updateResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.Equal("A handbook of agile software craftsmanship.", updatedBook?.Description);

        var deleteResponse = await client.DeleteAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await client.GetAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    private static async Task<AuthResponse> Register(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Username = $"test-{Guid.NewGuid():N}",
            Password = "correct-horse-battery-staple"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return Assert.IsType<AuthResponse>(auth);
    }

    private static async Task AssertSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var authenticationHeaders = string.Join(", ", response.Headers.WwwAuthenticate);
        Assert.Fail($"Expected success but received {(int)response.StatusCode}. WWW-Authenticate: {authenticationHeaders}. Body: {body}");
    }
}
