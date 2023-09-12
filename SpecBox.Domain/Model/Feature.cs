using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("Feature")]
public class Feature
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public List<AttributeValue> Attributes { get; } = new();

    public List<AssertionGroup> AssertionGroups { get; } = new();
    
    public List<TreeNode> TreeNodes { get; } = new();
}
