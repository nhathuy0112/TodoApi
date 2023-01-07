using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Dto.Auth;
using TodoApi.Errors;
using TodoApi.Models.Identity;
using TodoApi.Services;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TodoApi.Controllers;

public class AuthController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly TodoDbContext _context;
    public AuthController(UserManager<User> userManager, IAuthService authService, IConfiguration configuration, TodoDbContext context)
    {
        _userManager = userManager;
        _authService = authService;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterInfo registerInfo)
    {
        var errors = new List<string>() { };
        var check = await _authService.CheckRegisterRequest(registerInfo, errors);

        if (check)
        {
            var newUser = new User()
            {
                Email = registerInfo.Email,
                UserName = registerInfo.Email,
                PhoneNumber = registerInfo.Phone,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            // Create new user in db
            var result = await _userManager.CreateAsync(newUser, registerInfo.Password);
            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new [] {"Cannot register"}
                });
            }
            // Add role
            if (registerInfo.IsAdmin)
            {
                var res = await _userManager.AddToRoleAsync(newUser, Role.ADMIN.ToString());
            }
            else
            {
                await _userManager.AddToRoleAsync(newUser, Role.USER.ToString());
            }
            
            return Ok(new RegisterResponse()
            {
                Email = registerInfo.Email,
                Username = registerInfo.Email,
                Phone = registerInfo.Phone,
                Role = registerInfo.IsAdmin ? "Admin" : "User"
            });
        }

        return new BadRequestObjectResult(new ApiValidationErrorResponse()
        {
            Errors = errors
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginInfo loginInfo)
    {
        var existedUser = await _userManager.FindByEmailAsync(loginInfo.Username);
        
        if (existedUser == null)
        {
            return Unauthorized(new ApiResponse(401, null));
        }

        var isCorrectPassword = await _userManager.CheckPasswordAsync(existedUser, loginInfo.Password);
        if (!isCorrectPassword)
        {
            return Unauthorized(new ApiResponse(401, null));
        }

        var response = await _authService.CreateLoginResponse(existedUser);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
    {
        var tokenValidationParameter = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:AccessTokenSecret").Value)),
            ValidateIssuer = false, //development
            ValidateAudience = false,//development
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        
            var tokenInVerification = tokenHandler.ValidateToken(refreshRequest.AccessToken, tokenValidationParameter,
                out var validatedToken);
            
            // Check access token algorithm
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, 
                    StringComparison.InvariantCultureIgnoreCase);
                if (result is false)
                {
                    return new BadRequestObjectResult(new ApiValidationErrorResponse()
                    {
                        Errors = new []{"Invalid access token"}
                    });
                }
            }
            
            // Check access token expiry date
            var utcExiryDate = long.Parse(tokenInVerification.Claims
                .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var expiryDate = dateTimeVal.AddSeconds(utcExiryDate).ToUniversalTime();
            if (expiryDate > DateTime.UtcNow)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new []{"Access token is not expired"}
                });
            }

            var existedToken = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == refreshRequest.RefreshToken);
            
            // Check refresh token in db
            if (existedToken is null)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new []{"Not found"}
                });
            }

            // Check refresh token is used
            if (existedToken.IsUsed)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new []{"Is used"}
                });
            }

            // Check refresh token is revoked
            if (existedToken.IsRevoked)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new []{"Is revoke"}
                });
            }

            // Check jti of access token in db
            var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            if (existedToken.JwtId != jti)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new []{"Invalid jti"}
                });
            }

            // Check expiry date of refresh token
            if (existedToken.ExpiryDate < DateTime.UtcNow)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse()
                {
                    Errors = new []{"Refresh token is expired"}
                });
            }

            existedToken.IsUsed = true;
            _context.RefreshTokens.Update(existedToken);
            await _context.SaveChangesAsync();

            // Create new pair of tokens and return
            var response = await _authService.CreateLoginResponse(existedToken.User);
            return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var existedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == request.CurrentRefreshToken);
        if (existedToken is null)
        {
            return new BadRequestObjectResult(new ApiValidationErrorResponse()
            {
                Errors = new[] { "Not found" }
            });
        }
        if (existedToken.IsUsed)
        {
            return new BadRequestObjectResult(new ApiValidationErrorResponse()
            {
                Errors = new []{"Is used"}
            });
        }
        if (existedToken.ExpiryDate < DateTime.UtcNow)
        {
            return new BadRequestObjectResult(new ApiValidationErrorResponse()
            {
                Errors = new []{"Is expired"}
            });
        }
        if (existedToken.IsRevoked)
        {
            return new BadRequestObjectResult(new ApiValidationErrorResponse()
            {
                Errors = new []{"Is revoked"}
            });
        }

        _context.RefreshTokens.Remove(existedToken);
        await _context.SaveChangesAsync();
        return Ok("Signed out");
    }
}