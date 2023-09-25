using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using SpecBox.Domain;
using SpecBox.Domain.Model;
using SpecBox.WebApi.Model.Upload;
using Attribute = SpecBox.Domain.Model.Attribute;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("export")]
public class ExportController : Controller
{
    private readonly SpecBoxDbContext db;
    private readonly ILogger logger;

    public ExportController(SpecBoxDbContext db, ILogger<ExportController> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromQuery(Name = "project")] string projectCode,
        [FromBody] UploadData data)
    {
        // получаем проект из БД
        var prj = await db.Projects.SingleAsync(p => p.Code == projectCode);

        // экспериментальный экспорт на стороне БД
        await RunExport(prj, data);

        // загружаем все данные для обработки в памяти
        var trees = await db.Trees.Include(t => t.AttributeGroupOrders).Where(t => t.ProjectId == prj.Id).ToListAsync();
        var features = await db.Features.Include(f => f.Attributes).Where(e => e.ProjectId == prj.Id).ToListAsync();
        var groups = await db.AssertionGroups.Where(e => e.Feature.ProjectId == prj.Id).ToListAsync();
        var assertions = await db.Assertions.Where(a => a.AssertionGroup.Feature.ProjectId == prj.Id).ToListAsync();
        var attributes = await db.Attributes.Where(a => a.ProjectId == prj.Id).ToListAsync();
        var values = await db.AttributeValues
            .Include(v => v.Attribute)
            .Where(a => a.Attribute.ProjectId == prj.Id)
            .ToListAsync();

        foreach (var a in data.Attributes)
        {
            var attribute = GetAttribute(a, attributes, prj);
            attribute.Title = a.Title;

            foreach (var v in a.Values)
            {
                var value = GetAttributeValue(v.Code, values, attribute);
                value.Title = v.Title;
            }
        }

        await db.SaveChangesAsync();

        foreach (var f in data.Features)
        {
            var feature = GetFeature(f, features, prj);
            feature.Title = f.Title;
            feature.Description = f.Description;
            feature.FilePath = f.FilePath;

            if (f.Attributes != null)
            {
                foreach (var attr in f.Attributes)
                {
                    foreach (var valCode in attr.Value)
                    {
                        if (!feature.Attributes.Any(a => a.Code == valCode && a.Attribute.Code == attr.Key))
                        {
                            var attribute = attributes.Single(a => a.Code == attr.Key);
                            var value = GetAttributeValue(valCode, values, attribute);

                            feature.Attributes.Add(value);
                        }
                    }
                }
            }

            foreach (var g in f.Groups)
            {
                var group = GetGroup(g, groups, feature);

                foreach (var a in g.Assertions)
                {
                    logger.LogInformation("process assertion: {Title}", a.Title);

                    var assertion = assertions.SingleOrDefault(x =>
                        x.AssertionGroup.Id == group.Id &&
                        StringComparer.InvariantCultureIgnoreCase.Equals(x.Title, a.Title));

                    if (assertion == null)
                    {
                        assertion = new Assertion
                        {
                            Id = Guid.NewGuid(),
                            Title = a.Title,
                            AssertionGroupId = group.Id,
                            AssertionGroup = group,
                        };

                        assertions.Add(assertion);
                        db.Assertions.Add(assertion);
                    }

                    assertion.Description = a.Description;
                    assertion.IsAutomated = a.IsAutomated;
                }
            }
        }

        // trees
        foreach (var t in data.Trees)
        {
            var tree = GetTree(t, trees, prj);
            tree.Title = t.Title;

            var order = 1;
            db.AttributeGroupOrders.RemoveRange(tree.AttributeGroupOrders);

            foreach (var attributeCode in t.Attributes)
            {
                var attribute = attributes.Single(att => att.Code == attributeCode);

                var obj = new AttributeGroupOrder
                {
                    Id = Guid.NewGuid(),
                    Attribute = attribute,
                    Order = order++,
                    Tree = tree,
                };

                db.AttributeGroupOrders.Add(obj);
                tree.AttributeGroupOrders.Add(obj);
            }
        }

        await db.SaveChangesAsync();

        await db.BuildTree(prj.Id);

        // stat
        var allAssertions = data.Features
            .SelectMany(f => f.Groups)
            .SelectMany(gr => gr.Assertions)
            .ToArray();

        var statRecord = new AssertionsStatRecord
        {
            Id = Guid.NewGuid(),
            ProjectId = prj.Id,
            Project = prj,
            Timestamp = DateTime.UtcNow,
            TotalCount = allAssertions.Length,
            AutomatedCount = allAssertions.Count(a => a.IsAutomated)
        };

        db.AssertionsStat.Add(statRecord);

        await db.SaveChangesAsync();

        return Ok();
    }

    private Attribute GetAttribute(AttributeModel model, List<Attribute> attributes, Project project)
    {
        logger.LogInformation("process attribute: {Code}", model.Code);

        var attribute = attributes.SingleOrDefault(f => f.Code == model.Code);

        if (attribute == null)
        {
            logger.LogInformation("attribute doesn't exist, it will be created");

            attribute = new Attribute
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Project = project,
                Code = model.Code,
            };

            db.Attributes.Add(attribute);
            attributes.Add(attribute);
        }

        return attribute;
    }

    private AttributeValue GetAttributeValue(string valueCode, List<AttributeValue> values, Attribute attribute)
    {
        logger.LogInformation("process attribute value: {Code}", valueCode);

        var value = values.SingleOrDefault(obj =>
            obj.Code == valueCode && obj.Attribute.Code == attribute.Code);

        if (value == null)
        {
            value = new AttributeValue
            {
                Id = Guid.NewGuid(),
                Code = valueCode,
                Title = valueCode,
                Attribute = attribute,
                AttributeId = attribute.Id
            };

            db.AttributeValues.Add(value);
            values.Add(value);
        }

        return value;
    }

    private Feature GetFeature(FeatureModel model, List<Feature> features, Project project)
    {
        logger.LogInformation("process feature: {Code}", model.Code);

        var feature = features.SingleOrDefault(f => f.Code == model.Code);

        if (feature == null)
        {
            logger.LogInformation("feature doesn't exist, it will be created");

            feature = new Feature
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Project = project,
                Code = model.Code,
            };

            db.Features.Add(feature);
            features.Add(feature);
        }

        return feature;
    }

    private AssertionGroup GetGroup(AssertionGroupModel model, List<AssertionGroup> groups, Feature feature)
    {
        logger.LogInformation("process assertion group: {Title}", model.Title);

        var group = groups.SingleOrDefault(obj => obj.Title == model.Title);

        if (group == null)
        {
            group = new AssertionGroup
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                FeatureId = feature.Id,
            };

            db.AssertionGroups.Add(group);
            groups.Add(group);
        }

        return group;
    }

    private Tree GetTree(TreeModel model, List<Tree> trees, Project project)
    {
        logger.LogInformation("process tree: {Title}", model.Title);

        var tree = trees.SingleOrDefault(t => t.Code == model.Code);

        if (tree == null)
        {
            tree = new Tree
            {
                Id = Guid.NewGuid(),
                Project = project,
                Code = model.Code
            };
            db.Trees.Add(tree);
            trees.Add(tree);
        }

        return tree;
    }

    private async Task RunExport(Project project, UploadData data)
    {
        var export = new Export { Id = Guid.NewGuid(), Project = project, Timestamp = DateTime.UtcNow };

        db.Exports.Add(export);
        await db.SaveChangesAsync();

        await using var conn = db.GetConnection();

        await conn.OpenAsync();

        await using var writer = await conn.BeginBinaryImportAsync(
            "COPY \"ExportFeature\" (\"ExportId\", \"Code\",\"Title\",\"Description\", \"FilePath\") FROM STDIN (FORMAT BINARY)"
        );

        foreach (var feature in data.Features)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(export.Id, NpgsqlDbType.Uuid);
            await writer.WriteAsync(feature.Code, NpgsqlDbType.Text);
            await writer.WriteAsync(feature.Title, NpgsqlDbType.Text);
            await writer.WriteAsync(feature.Description, NpgsqlDbType.Text);
            await writer.WriteAsync(feature.FilePath, NpgsqlDbType.Text);
        }

        await writer.CompleteAsync();
    }
}
