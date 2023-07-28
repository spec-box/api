using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("Assertion")]
public class Assertion
{
    public Guid Id { get; set; }
    
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsAutomated { get; set; }

    public Guid AssertionGroupId { get; set; }
    public AssertionGroup AssertionGroup { get; set; } = null!;
}
