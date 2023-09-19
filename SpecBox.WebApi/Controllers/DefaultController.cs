using Microsoft.AspNetCore.Mvc;

namespace SpecBox.WebApi.Controllers;

public class DefaultController : Controller
{
    [HttpGet("ping")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Ping()
    {
        // ручка для проверки работоспособности приложения
        return Ok();
    }
}
