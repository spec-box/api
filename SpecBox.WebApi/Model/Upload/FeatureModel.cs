using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Upload;

public class FeatureModel
{
    [Required] public string Code { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    public string? Description { get; set; }
    
    public string? FilePath { get; set; }

    [Required] public AssertionGroupModel[] Groups { get; set; } = null!;

    public Dictionary<string, string[]>? Attributes { get; set; }
}
