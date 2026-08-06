using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using SpecBox.Domain;
using SpecBox.Domain.Model;
using SpecBox.Domain.Model.Enums;
using SpecBox.WebApi.Model.Common;
using SpecBox.WebApi.Model.Project;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("projects")]
public class ProjectController(SpecBoxDbContext db, ILogger<ProjectController> logger, IMapper mapper)
    : Controller
{
    private readonly ILogger logger = logger;

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
    public IActionResult Feature(string project, string feature)
    {
        var f = db.Features
            .Include(f => f.AssertionGroups.OrderBy(g => g.SortOrder))
            .ThenInclude(g => g.Assertions.OrderBy(a => a.SortOrder))
            .Single(f => f.Code == feature && f.Project.Code == project);

        var model = mapper.Map<FeatureModel>(f);
        
        model.Usages = db.FeatureDependencies
            .Where(d => d.DependencyFeatureId == f.Id)
            .Include(t => t.SourceFeature)
            .ThenInclude(t => t.AssertionGroups)
            .ThenInclude(t => t.Assertions)
            .Select(t => t.SourceFeature)
            .Select(d => new RelatedFeatureModel {
                Code = d.Code,
                Title = d.Title,
                FeatureType = d.FeatureType,
                TotalCount = d.AssertionGroups.SelectMany(gr => gr.Assertions).Count(),
                AutomatedCount = d.AssertionGroups
                    .SelectMany(gr => gr.Assertions)
                    .Count(a => a.AutomationState == AutomationState.Automated),
                ProblemCount = d.AssertionGroups
                    .SelectMany(gr => gr.Assertions)
                    .Count(a => a.AutomationState == AutomationState.Problem),
            }).ToList();
        
        return Json(model);
    }

    [HttpGet("{project}/structure")]
    [ProducesResponseType(typeof(StructureModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Structure(string project, [FromQuery] StructureFilterModel filter)
    {
        var prj = await db.Projects.SingleAsync(p => p.Code == project);
        var tree = await db.Trees.FirstOrDefaultAsync(t => t.ProjectId == prj.Id);

        var projectModel = mapper.Map<Project, ProjectModel>(prj);

        var nodes = await GetTreeModel(prj.Id, tree?.Id, filter);

        var model = new StructureModel
        {
            Project = projectModel,
            Tree = nodes
        };

        return Json(model);
    }

    // Фильтрация и сборка результата (совпавшие узлы + их предки, совпадение группы
    // по названию распространяется на поддерево) выполняются функцией FilterTree в БД,
    // см. SpecBox.Migrations/Resources/019_FilterTree.sql. Она же строит плоский список
    // фич с подсчётом покрытия, когда у проекта нет настроенных деревьев (treeId = null).
    private Task<TreeNodeModel[]> GetTreeModel(Guid projectId, Guid? treeId, StructureFilterModel filter)
    {
        var search = filter.Title is { Length: > 0 } title ? title : null;

        return db.Database
            .SqlQueryRaw<TreeNodeModel>(
                """
                SELECT
                    "Id", "ParentId", "Title", "SortOrder", "FeatureCode", "FeatureType", "IsExpanded",
                    "Amount" AS "TotalCount",
                    "AmountAutomated" AS "AutomatedCount",
                    "AmountProblem" AS "ProblemCount"
                FROM "FilterTree"(@projectId, @treeId, @search, @featureType, @hasProblems)
                """,
                new NpgsqlParameter("projectId", NpgsqlDbType.Uuid) { Value = projectId },
                new NpgsqlParameter("treeId", NpgsqlDbType.Uuid) { Value = (object?)treeId ?? DBNull.Value },
                new NpgsqlParameter("search", NpgsqlDbType.Varchar) { Value = (object?)search ?? DBNull.Value },
                new NpgsqlParameter("featureType", NpgsqlDbType.Integer) { Value = (object?)(int?)filter.FeatureType ?? DBNull.Value },
                new NpgsqlParameter("hasProblems", NpgsqlDbType.Boolean) { Value = (object?)filter.HasProblems ?? DBNull.Value })
            .ToArrayAsync();
    }
}
