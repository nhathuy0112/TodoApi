using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dto.Auth;
using TodoApi.Errors;
using TodoApi.Models.Identity;
using TodoApi.Services;

namespace TodoApi.Controllers;

public class AuthController : BaseController
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthValidator _authValidator;
    private readonly ITokenService _tokenService;
    
    public AuthController(UserManager<User> userManager, IAuthValidator authValidator, ITokenService tokenService)
    {
        _userManager = userManager;
        _authValidator = authValidator;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterInfo registerInfo)
    {
        var errors = new List<string>() { };
        var check = await _authValidator.CheckRegisterRequest(registerInfo, errors);

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

        var accessToken = await _tokenService.CreateToken(existedUser, TokenType.Access);
        var refreshToken = await _tokenService.CreateToken(existedUser, TokenType.Refresh);

        return Ok(new LoginResponse()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
}