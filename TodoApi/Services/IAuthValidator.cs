using TodoApi.Dto.Auth;

namespace TodoApi.Services;

public interface IAuthValidator
{
    Task<bool> CheckRegisterRequest(RegisterInfo registerInfo, List<string> errors);
}