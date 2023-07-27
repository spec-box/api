using Microsoft.AspNetCore.Mvc;
using tms.Domain;
using tms.Domain.Model;
using tms.WebApi.Model;

namespace tms.WebApi.Controllers;

[ApiController, Microsoft.AspNetCore.Mvc.Route("api")]
public class MainController : Controller
{
    private readonly TmsDbContext db;

    public MainController(TmsDbContext db)
    {
        this.db = db;
    }

    [HttpGet("projects")]
    public IActionResult Projects()
    {
        var projects = db.Projects.ToArray();

        return Json(projects);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromQuery] string project, [FromBody] UploadData data)
    {
        var prj = db.Projects.Single(p => p.Code == project);

        foreach (var e in data.Entities)
        {
            var entity = new Entity
            {
                Id = Guid.NewGuid(), 
                ProjectId = prj.Id, 
                Code = e.Code, 
                Title = e.Title,
                Description = e.Description,
            };
            
            db.Entities.Add(entity);
        }

        await db.SaveChangesAsync();

        return Ok();
    }
}
