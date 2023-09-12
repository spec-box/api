using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("TreeNode")]
public class TreeNode
{
    public Guid Id { get; set; }

    public Guid? AttributeValueId { get; set; } = null!;
    public AttributeValue AttributeValue { get; set; } = null!;

    public Guid TreeId { get; set; }
    public Tree Tree { get; set; } = null!;

    public Guid? ParentId { get; set; } = null!;
    public TreeNode Parent { get; set; } = null!;

    public List<Feature> Features { get; set; } = null!;
}
[Table("TreeNodeFeature")]
internal class TreeNodeFeature
{
    public Guid TreeNodeId { get; set; }
    public Guid FeatureId { get; set; }
}
