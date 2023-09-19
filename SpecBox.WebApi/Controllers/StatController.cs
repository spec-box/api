using AutoMapper;
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
    private readonly IMapper mapper;

    public StatController(SpecBoxDbContext db, ILogger<StatController> logger, IMapper mapper)
    {
        this.db = db;
        this.logger = logger;
        this.mapper = mapper;
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
        [FromQuery(Name = "from")] DateTime? dateFrom,
        [FromQuery(Name = "to")] DateTime? dateTo)
    {
        var project = await db.Projects.SingleAsync(p => p.Code == projectCode);
        var from = NormalizeDateFrom(dateFrom);
        var to = NormalizeDateTo(dateTo);

        var stat = db.AutotestsStat
            .Where(assertion =>
                assertion.ProjectId == project.Id &&
                assertion.Timestamp >= from &&
                assertion.Timestamp < to)
            .ToArray();

        return Json(stat.Select(mapper.Map<AutotestsStatModel>));
    }

    [HttpGet("assertions")]
    public async Task<IActionResult> GetAssertionsStat(
        [FromQuery(Name = "project")] string projectCode,
        [FromQuery(Name = "from")] DateTime? dateFrom,
        [FromQuery(Name = "to")] DateTime? dateTo)
    {
        var project = await db.Projects.SingleAsync(p => p.Code == projectCode);
        var from = NormalizeDateFrom(dateFrom);
        var to = NormalizeDateTo(dateTo);

        var stat = db.AssertionsStat
            .Where(assertion =>
                assertion.ProjectId == project.Id &&
                assertion.Timestamp >= from &&
                assertion.Timestamp < to)
            .ToArray();

        return Json(stat.Select(mapper.Map<AssertionsStatModel>));
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
