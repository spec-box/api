using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("TreeNode")]
public class TreeNode
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;
    public int Amount { get; set; }
    public int AmountAutomated { get; set; }

    public Guid? FeatureId { get; set; } = null!;
    public Feature Feature { get; set; } = null!;

    public Guid TreeId { get; set; }
    public Tree Tree { get; set; } = null!;

    public Guid? ParentId { get; set; } = null!;
    public TreeNode Parent { get; set; } = null!;
}
