using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("AssertionGroup")]
public class AssertionGroup
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    
    public int? SortOrder { get; set; }

    public Guid FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
    
    public List<Assertion> Assertions { get; set; } = new();
}
