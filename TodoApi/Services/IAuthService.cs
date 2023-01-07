using TodoApi.Dto.Auth;
using TodoApi.Models.Identity;

namespace TodoApi.Services;

public interface IAuthService
{
    Task<LoginResponse> CreateLoginResponse(User user);
    Task<bool> CheckRegisterRequest(RegisterInfo registerInfo, List<string> errors);
}