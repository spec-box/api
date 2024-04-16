using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain;
using SpecBox.Domain.BulkCopy;
using SpecBox.Domain.Model;
using SpecBox.Domain.Model.Enums;
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
        await using var tran = await db.Database.BeginTransactionAsync();

        // получаем проект из БД
        var prj = await db.Projects.SingleAsync(p => p.Code == projectCode);

        // экспорт атрибутов и значений
        var attributes = await db.Attributes.Where(a => a.ProjectId == prj.Id).ToListAsync();
        var values = await db.AttributeValues
            .Include(v => v.Attribute)
            .Where(a => a.Attribute.ProjectId == prj.Id)
            .ToListAsync();

        foreach (var a in data.Attributes)
        {
            var attribute = GetAttribute(a, attributes, prj);
            attribute.Title = a.Title;
            var sortOrder = 1;

            foreach (var v in a.Values)
            {
                var value = GetAttributeValue(v.Code, values, attribute);
                value.Title = v.Title;
                value.SortOrder = sortOrder++;
            }
        }

        await db.SaveChangesAsync();

        // экспорт фичей на стороне БД
        await RunExport(prj, data);

        // экспорт деревьев
        var trees = await db.Trees
            .Include(t => t.AttributeGroupOrders)
            .Where(t => t.ProjectId == prj.Id)
            .ToListAsync();

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

        // формируем деревья
        await db.BuildTree(prj.Id);

        // сохраняем статистику
        await WriteStat(prj.Id, data);

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
        // добавляем новый экспорт
        var export = new Export
            { Id = Guid.NewGuid(), Project = project, Timestamp = DateTime.UtcNow, Message = data.Message };

        db.Exports.Add(export);
        await db.SaveChangesAsync();

        // сохраняем данные в таблицу
        var connection = await db.GetConnection();

        await using (var featureWriter = connection.CreateFeatureWriter())
        {
            // экспорт фичей
            foreach (var feature in data.Features)
            {
                await featureWriter.AddFeature(
                    export.Id,
                    feature.Code,
                    feature.Title,
                    feature.Description,
                    feature.FeatureType,
                    feature.FilePath);
            }

            await featureWriter.CompleteAsync();
        }

        // экспорт зависимостей
        await using (var dependencyWriter = connection.CreateFeatureDependencyWriter())
        {
            // экспорт фичей
            foreach (var feature in data.Features)
            {
                logger.LogInformation("process feature: {Code}", feature);
                if (feature.Dependencies == null)
                {
                    logger.LogInformation("empty dependecies! feature: {Code}", feature.Code);
                    continue;
                }

                foreach (var dependencyCode in feature.Dependencies)
                {
                    await dependencyWriter.AddFeatureDependency(
                        export.Id,
                        feature.Code,
                        dependencyCode);
                }
            }

            await dependencyWriter.CompleteAsync();
        }

        // экспорт утверждений
        await using (var assertionWriter = connection.CreateAssertionWriter())
        {
            foreach (var feature in data.Features)
            {
                var groupSortOrder = 0;

                foreach (var group in feature.Groups)
                {
                    var assertionSortOrder = 0;

                    foreach (var assertion in group.Assertions)
                    {
                        await assertionWriter.AddAssertion(
                            export.Id,
                            feature.Code,
                            group.Title,
                            groupSortOrder,
                            assertion.Title,
                            assertion.Description,
                            assertionSortOrder,
                            GetAutomationState(assertion));

                        assertionSortOrder++;
                    }

                    groupSortOrder++;
                }
            }

            await assertionWriter.CompleteAsync();
        }

        // экспорт атрибутов фичей
        await using (var featureAttributeWriter = connection.CreateFeatureAttributeWriter())
        {
            foreach (var feature in data.Features)
            {
                if (feature.Attributes == null) continue;

                foreach (var attribute in feature.Attributes)
                {
                    var attributeCode = attribute.Key;

                    foreach (var valueCode in attribute.Value)
                    {
                        await featureAttributeWriter.AddFeatureAttribute(
                            export.Id,
                            feature.Code,
                            attributeCode,
                            valueCode
                        );
                    }
                }
            }

            await featureAttributeWriter.CompleteAsync();
        }

        // запускаем обработку данных
        await db.MergeExportedData(export.Id);
    }

    private AutomationState GetAutomationState(AssertionModel assertion)
    {
        return assertion.AutomationState ??
               (assertion.IsAutomated == true
                   ? AutomationState.Automated
                   : AutomationState.Unknown);
    }

    private async Task WriteStat(Guid projectId, UploadData data)
    {
        // stat
        var assertionsState = data.Features
            .SelectMany(f => f.Groups)
            .SelectMany(gr => gr.Assertions)
            .Select(GetAutomationState)
            .ToArray();

        var statRecord = new AssertionsStatRecord
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow,
            TotalCount = assertionsState.Length,
            AutomatedCount = assertionsState.Count(a => a == AutomationState.Automated),
            ProblemCount = assertionsState.Count(a => a == AutomationState.Problem),
        };

        db.AssertionsStat.Add(statRecord);

        await db.SaveChangesAsync();
    }
}
