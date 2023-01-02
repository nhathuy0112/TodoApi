using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dto.Auth;

public class LoginInfo
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}