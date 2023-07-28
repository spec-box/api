using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("AttributeValue")]
public class AttributeValue
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }

    public Guid AttributeId { get; set; }
    public Attribute Attribute { get; set; } = null!;
    
    public List<Feature> Features { get; } = new();
}

internal class FeatureAttributeValue
{
    public Guid AttributeValueId { get; set; }
    public Guid FeatureId { get; set; }
}
