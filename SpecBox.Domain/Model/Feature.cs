using System.ComponentModel.DataAnnotations.Schema;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.Model;

[Table("Feature")]
public class Feature
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;

    public FeatureType? FeatureType { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public string? FilePath { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public List<FeatureDependency> Dependencies { get; } = new();

    public List<AttributeValue> Attributes { get; } = new();

    public List<AssertionGroup> AssertionGroups { get; } = new();
}
