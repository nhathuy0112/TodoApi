using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dto.Auth;

public class RefreshRequest
{
    [Required]
    public string AccessToken { get; set; }
    [Required]
    public string RefreshToken { get; set; }
}