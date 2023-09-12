using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("AttributeGroupOrder")]
public class AttributeGroupOrder
{
    public Guid Id { get; set; }
    public int Order { get; set; }

    public Guid TreeId { get; set; }
    public Tree Tree { get; set; } = null!;

    public Guid AttributeId { get; set; }
    public Attribute Attribute { get; set; } = null!;
}
