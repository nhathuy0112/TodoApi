namespace TodoApi.Models;

public class IConfiguration
{
    public string AccessTokenSecret { get; set; }
    public double AccessTokenExpirationMinutes { get; set; }
    public string RefreshTokenSecret { get; set; }
    public double RefreshTokenExpirationMinutes { get; set; }
    
}