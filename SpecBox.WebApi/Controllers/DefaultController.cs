using Microsoft.AspNetCore.Mvc;

namespace SpecBox.WebApi.Controllers;

public class DefaultController : Controller
{
    [HttpGet("ping")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Ping()
    {
        return Ok();
    }
}
