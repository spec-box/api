using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.WebApi.Model.Project;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("api/projects")]
public class ProjectController : Controller
{
    private readonly SpecBoxDbContext db;
    private readonly ILogger logger;
    private readonly IMapper mapper;
    
    public ProjectController(SpecBoxDbContext db, ILogger<ProjectController> logger, IMapper mapper)
    {
        this.db = db;
        this.logger = logger;
        this.mapper = mapper;
    }
    
    [HttpGet("{project}/features/{feature}")]
    [ProducesResponseType(typeof(FeatureModel), StatusCodes.Status200OK)]
    public IActionResult Feature(string project, string feature)
    {
        var f = db.Features
            .Include(f => f.AssertionGroups)
            .ThenInclude(g => g.Assertions)
            .SingleOrDefault(f => f.Code == feature && f.Project.Code == project);

        var model = mapper.Map<FeatureModel>(f);

        return Json(model);
    }
}
