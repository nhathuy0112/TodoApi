using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoApi.Dto.Auth;
using TodoApi.Models.Identity;

namespace TodoApi.Services;

public class AuthValidator : IAuthValidator
{
    private readonly UserManager<User> _userManager;

    public AuthValidator(UserManager<User> userManager)
    {
        _userManager = userManager;
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
}