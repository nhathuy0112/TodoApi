using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Models.Identity;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TodoApi.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly TodoDbContext _context;
    
    public TokenService(IConfiguration configuration, TodoDbContext context, UserManager<User> userManager)
    {
        _configuration = configuration;
        _context = context;
        _userManager = userManager;
    }

    public async Task<string> CreateToken(User user, TokenType type)
    {
        var isAccessToken = type == TokenType.Access;
        
        //Add information of user, here is only id and role
        var claims = new List<Claim>()
        {
            new Claim("Id", user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
        };
        //Add user role into claims list above
        claims.AddRange(
            from userRole in await _userManager.GetRolesAsync(user) 
            select new Claim(ClaimTypes.Role, userRole));

        //Encode secret key
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            isAccessToken ? 
                _configuration["Jwt:AccessTokenSecret"] 
                : _configuration["Jwt:RefreshTokenSecret"]));
                    

        //Create credential
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        //Describe token
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = isAccessToken ? new ClaimsIdentity(claims) : null,
            Expires = isAccessToken
                ? DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]))
                : DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:RefreshTokenExpirationMinutes"])),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        //Generate token and return
        return tokenHandler.WriteToken(token);
    }

    public async Task SaveToken(string userId, string token, TokenType type)
    {
        throw new Exception();
    }
}