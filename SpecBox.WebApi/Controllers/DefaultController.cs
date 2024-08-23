using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpecBox.WebApi.Model.Default;

namespace SpecBox.WebApi.Controllers;

public class DefaultController(IConfiguration configuration, IOptions<JsonOptions> jsonOptions)
    : Controller
{
    private readonly JsonOptions jsonOptions = jsonOptions.Value;

    [HttpGet("ping"), ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Ping()
    {
        // ручка для проверки работоспособности приложения
        return Ok();
    }

    [HttpGet("config")]
    [ProducesResponseType(typeof(ConfigurationModel), StatusCodes.Status200OK)]
    public IActionResult Config()
    {
        return Json(GetConfigModel());
    }

    [HttpGet("config.js"), ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult ConfigScript()
    {
        var configModel = GetConfigModel();
        var json = JsonSerializer.Serialize(configModel, jsonOptions.JsonSerializerOptions);

        return Content($"window.__SPEC_BOX_CONFIG={json}", "application/javascript");
    }

    private ConfigurationModel GetConfigModel()
    {
        string? counterId = configuration.GetValue<string>("MetrikaCounterId");
        ConfigurationModel model = new() { MetrikaCounterId = counterId };

        return model;
    }
}
