using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.Domain.Model;
using SpecBox.WebApi.Model.Upload;
using Attribute = SpecBox.Domain.Model.Attribute;

namespace SpecBox.WebApi.Controllers;

[ApiController, Route("api/export")]
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
        await using var tran = await db.Database.BeginTransactionAsync();

        var prj = await db.Projects.SingleAsync(p => p.Code == projectCode);
        var features = await db.Features.Include(f => f.Attributes).Where(e => e.ProjectId == prj.Id).ToListAsync();
        var groups = await db.AssertionGroups.Where(e => e.Feature.ProjectId == prj.Id).ToListAsync();
        var assertions = await db.Assertions.Where(a => a.AssertionGroup.Feature.ProjectId == prj.Id).ToListAsync();
        var attributes = await db.Attributes.Where(a => a.ProjectId == prj.Id).ToListAsync();
        var trees = await db.Trees.Where(t=>t.ProjectId == prj.Id).ToListAsync();
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

        if (data.Trees != null)
        {
            foreach (var t in data.Trees)
            {
                var tree = GetTree(t, trees, prj);
                if (t.Attributes != null)
                {
                    var order = 1;
                    tree.AttributeGroupOrders.AddRange(t.Attributes.Select(a => new AttributeGroupOrder
                    {
                        Attribute = attributes.Single(att => att.Code == a),
                        Order = order++,
                        Tree = tree,
                    }));
                }
                db.Trees.Add(tree);
            }
        }

        await db.BuildTree(prj.Id);

        await db.SaveChangesAsync();

        await tran.CommitAsync();

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
                Project = project,
                Code = model.Code,
                Title = model.Title
            };
            db.Trees.Add(tree);
            trees.Add(tree);
        }
        return tree;
    }
}
