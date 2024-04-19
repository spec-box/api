using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.Domain.Model;
using SpecBox.Domain.Model.Enums;
using SpecBox.WebApi.Model.Common;
using SpecBox.WebApi.Model.Project;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("projects")]
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

    [HttpGet("list")]
    [ProducesResponseType(typeof(ProjectModel[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> Projects()
    {
        var projects = await db.Projects.ToArrayAsync();

        var model = projects.Select(mapper.Map<Project, ProjectModel>).ToArray();

        return Json(model);
    }

    [HttpGet("{project}/features/{feature}")]
    [ProducesResponseType(typeof(FeatureModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Feature(string project, string feature)
    {
        var f = db.Features
            .Include(f => f.AssertionGroups.OrderBy(g => g.SortOrder))
            .ThenInclude(g => g.Assertions.OrderBy(a => a.SortOrder))
            .SingleOrDefault(f => f.Code == feature && f.Project.Code == project);

        var model = mapper.Map<FeatureModel>(f);
        var deps = await db.FeatureDependencies
            .Where(d => d.SourceFeatureId == f.Id)
            .Join(db.Features,
                d => d.DependencyFeatureId,
                f => f.Id,
                (d, f) => new FeatureDependencyModel
                {
                    Code = f.Code,
                    Title = f.Title,
                    FeatureType = f.FeatureType,
                    AssertionsCount = 0,
                    AutomatedCount = 0,
                }
            )
            .ToListAsync();
        
        // logger.LogInformation("Deps: {}", deps);
        model.Dependencies = deps; 
        
        return Json(model);
    }

    [HttpGet("{project}/{treeCode}/structure")]
    [ProducesResponseType(typeof(StructureModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Structure(string project, string treeCode)
    {
        var prj = await db.Projects.SingleAsync(p => p.Code == project);
        var tree = await db.Trees.FirstOrDefaultAsync(t => t.ProjectId == prj.Id && (treeCode == "" || t.Code == treeCode));

        var projectModel = mapper.Map<Project, ProjectModel>(prj);

        var nodes = tree == null
            ? await GetDefaultTreeModel(project)
            : await GetTreeModel(tree);

        var model = new StructureModel
        {
            Project = projectModel,
            Tree = nodes
        };

        return Json(model);
    }

    [HttpGet("{project}/trees")]
    [ProducesResponseType(typeof(TreeGroupModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrees(string project)
    {
        var prj = await db.Projects.SingleAsync(p => p.Code == project);
        var trees = await  db.Trees.Where(t => t.ProjectId == prj.Id).ToArrayAsync();
        var new_trees = trees.Select(tree => new TreeModel {
            Code = tree.Code,
            Title = tree.Title,
        }).ToArray();

        var projectModel = mapper.Map<Project, ProjectModel>(prj);
        var model = new TreeGroupModel
        {
            Project = projectModel,
            Trees = new_trees
        };

        return Json(model);
    }

    private async Task<TreeNodeModel[]> GetDefaultTreeModel(string projectCode)
    {
        var nodes = await db.Features
            .Where(f => f.Project.Code == projectCode)
            .Select(f => new TreeNodeModel
            {
                Id = f.Id,
                Title = f.Title,
                FeatureCode = f.Code,
                FeatureType = f.FeatureType,
                TotalCount = f.AssertionGroups.SelectMany(gr => gr.Assertions).Count(),
                AutomatedCount = f.AssertionGroups.SelectMany(gr => gr.Assertions)
                    .Count(a => a.AutomationState == AutomationState.Automated),
                ProblemCount = f.AssertionGroups.SelectMany(gr => gr.Assertions)
                    .Count(a => a.AutomationState == AutomationState.Problem),
                
            })
            .ToArrayAsync();

        return nodes;
    }

    private async Task<TreeNodeModel[]> GetTreeModel(Tree tree)
    {
        var nodes = await db.TreeNodes
            .Where(n => n.TreeId == tree.Id)
            .Select(n => new TreeNodeModel
            {
                Id = n.Id,
                ParentId = n.ParentId,
                Title = n.Title,
                TotalCount = n.Amount,
                AutomatedCount = n.AmountAutomated,
                ProblemCount = n.AmountProblem,
                FeatureCode = n.Feature == null ? null : n.Feature.Code,
                FeatureType = n.Feature == null ? null : n.Feature.FeatureType,
                SortOrder = n.SortOrder,
            })
            .ToArrayAsync();

        return nodes;
    }
}
