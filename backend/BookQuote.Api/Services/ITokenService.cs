using BookQuote.Api.Models;

namespace BookQuote.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
