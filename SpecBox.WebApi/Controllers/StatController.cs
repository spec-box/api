using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.Domain.Model;
using SpecBox.WebApi.Model.Common;
using SpecBox.WebApi.Model.Stat;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("stat")]
public class StatController(SpecBoxDbContext db, ILogger<StatController> logger, IMapper mapper)
    : Controller
{
    private readonly ILogger logger = logger;

    [HttpPost("upload-autotests")]
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

    [HttpGet]
    [ProducesResponseType(typeof(StatModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAutotestsStat(
        [FromQuery(Name = "project")] string projectCode,
        [FromQuery(Name = "from")] DateTime? dateFrom,
        [FromQuery(Name = "to")] DateTime? dateTo)
    {
        var project = await db.Projects.SingleAsync(p => p.Code == projectCode);
        var from = NormalizeDateFrom(dateFrom);
        var to = NormalizeDateTo(dateTo);
        
        var assertions = await db.AssertionsStat
            .Where(assertion =>
                assertion.ProjectId == project.Id &&
                assertion.Timestamp >= from &&
                assertion.Timestamp < to)
            .OrderBy(s => s.Timestamp)
            .ToArrayAsync();
        
        var autotests = await db.AutotestsStat
            .Where(assertion =>
                assertion.ProjectId == project.Id &&
                assertion.Timestamp >= from &&
                assertion.Timestamp < to)
            .OrderBy(s => s.Timestamp)
            .ToArrayAsync();

        var model = new StatModel
        {
            Assertions = assertions.Select(mapper.Map<AssertionsStatModel>).ToArray(),
            Autotests = autotests.Select(mapper.Map<AutotestsStatModel>).ToArray(),
            Project = mapper.Map<ProjectModel>(project)
        };
        
        return Json(model);
    }
    
    private static DateTime NormalizeDateFrom(DateTime? dateFrom)
    {
        // если значение не задано, то отдаем данные за последние 3 месяца
        return DateTime.SpecifyKind(dateFrom.GetValueOrDefault(DateTime.Today.AddMonths(-3)), DateTimeKind.Utc);
    }

    private static DateTime NormalizeDateTo(DateTime? dateTo)
    {
        // отдаем данные до указанного  дня включительно
        // если день не указан, берем текущий день
        return DateTime.SpecifyKind(dateTo.GetValueOrDefault(DateTime.Today).AddDays(1), DateTimeKind.Utc);
    }
}
