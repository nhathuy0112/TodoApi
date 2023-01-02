using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers;

public class SimpleController : BaseController
{
    [HttpGet("onlyAdmin")]
    [Authorize(Roles = "ADMIN")]
    public ActionResult<string> Get()
    {
        return Ok($"Only admin can see this");
    }

    [HttpGet("onlyUser")]
    [Authorize(Roles = "USER")]
    public IActionResult Test()
    {
        var id = User.FindFirstValue("id");
        return Ok($"Only normal user can see this");
    }
}