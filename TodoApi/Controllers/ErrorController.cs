using Microsoft.AspNetCore.Mvc;
using TodoApi.Errors;

namespace TodoApi.Controllers;

[Route("errors/{code:int}")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : BaseController
{
    public IActionResult Error(int code)
    {
        return new ObjectResult(new ApiResponse(code, null));
    }
}