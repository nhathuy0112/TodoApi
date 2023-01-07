using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Dto.Auth;
using TodoApi.Models.Identity;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TodoApi.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly TodoDbContext _context;

    public AuthService(IConfiguration configuration, UserManager<User> userManager, TodoDbContext context)
    {
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
    }

    public async Task<LoginResponse> CreateLoginResponse(User user)
    {
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
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:AccessTokenSecret"]));
                    

        //Create credential
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

        //Describe token
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"])),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        
        // Generate a new refresh token
        var refreshTokenString = CreateRandomString();
        
        //Save new refresh token to db
        var refreshTokenModel = new RefreshToken()
        {
            JwtId = accessToken.Id,
            Token = refreshTokenString,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:RefreshTokenExpirationMonths"])),
            IsRevoked = false,
            IsUsed = false,
            UserId = user.Id
        };

        await _context.RefreshTokens.AddAsync(refreshTokenModel);
        await _context.SaveChangesAsync();
        
        return new LoginResponse()
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            RefreshToken = refreshTokenString
        };
    }

    public async Task<bool> CheckRegisterRequest(RegisterInfo registerInfo, List<string> errors)
    {
        var check = true;
        var existedUserByEmail = await _userManager.FindByEmailAsync(registerInfo.Email);
        var existedUserByPhone = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == registerInfo.Phone);
        if (existedUserByEmail != null)
        {
            errors.Add("Email is in used");
            check = false;
        }
        if (existedUserByPhone != null)
        {
            errors.Add("Phone number is in used");
            check = false;
        }

        if (registerInfo.Password != registerInfo.Password)
        {
            errors.Add("Confirm password is wrong");
            check = false;
        }

        return check;
    }

    private string CreateRandomString()
    {
        var random = new Random();
        var guid = Guid.NewGuid();
        var chars = guid + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" + DateTime.UtcNow.ToString();
        return new string(
            Enumerable.
                Repeat(chars, 100).
                Select(s => s[random.Next(s.Length)]).ToArray());
    }
}