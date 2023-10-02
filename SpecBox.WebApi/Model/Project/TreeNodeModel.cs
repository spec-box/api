using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class TreeNodeModel
{
    [Required] public Guid Id { get; set; }

    public Guid? ParentId { get; set; }
    
    public string? FeatureCode { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    [Required] public int TotalCount { get; set; }
    
    [Required] public int AutomatedCount { get; set; }
}
