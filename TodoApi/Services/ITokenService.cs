using TodoApi.Models.Identity;

namespace TodoApi.Services;

public interface ITokenService
{
    Task<string> CreateToken(User user, TokenType type);
    Task SaveToken(string userId, string token, TokenType type);
}