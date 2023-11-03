using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class FeatureModel
{
    [Required] public string Code { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    public string? Description { get; set; }
    
    public string? FilePath { get; set; }
    
    [Required] public List<AssertionGroupModel> AssertionGroups { get; } = new();
}
