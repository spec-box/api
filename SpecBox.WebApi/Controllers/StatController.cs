using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.Domain.Model;
using SpecBox.WebApi.Model.Stat;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("stat")]
public class StatController : Controller
{
    private readonly SpecBoxDbContext db;
    private readonly ILogger logger;

    public StatController(SpecBoxDbContext db, ILogger<StatController> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    [HttpPost("autotests/upload")]
    public async Task<IActionResult> AutotestsStatUpload(
        [FromQuery(Name = "project")] string projectCode,
        [FromBody] AutotestsStatUploadData data)
    {
        var project = await db.Projects.SingleAsync(p => p.Code == projectCode);

        var statRecord = new AutotestsStatRecord
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Project = project,
            Timestamp = data.Timestamp,
            AssertionsCount = data.AssertionsCount,
            Duration = data.Duration,
        };

        db.AutotestsStat.Add(statRecord);

        await db.SaveChangesAsync();

        return Ok();
    }
    
    [HttpGet("autotests")]
    public async Task<IActionResult> GetAutotestsStat(
        [FromQuery(Name = "project")] string projectCode,
        [FromQuery(Name = "from")] string? dateFrom,
        [FromQuery(Name = "to")] string? dateTo)
    {
        var project = await db.Projects.SingleAsync(p => p.Code == projectCode);


        return Ok();
    }

    [HttpGet("assertions")]
    public async Task<IActionResult> GetAssertionsStat(
        [FromQuery(Name = "project")] string projectCode,
        [FromQuery(Name = "from")] DateTime? dateFrom,
        [FromQuery(Name = "to")] DateTime? dateTo)
    {
        var project = await db.Projects.SingleAsync(p => p.Code == projectCode);


        return Ok();
    }
}
