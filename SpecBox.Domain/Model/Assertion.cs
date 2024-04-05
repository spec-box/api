using System.ComponentModel.DataAnnotations.Schema;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.Model;

[Table("Assertion")]
public class Assertion
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    
    public int? SortOrder { get; set; }

    public AutomationState AutomationState { get; set; }

    public Guid AssertionGroupId { get; set; }
    public AssertionGroup AssertionGroup { get; set; } = null!;
}
