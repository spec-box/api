using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class FeatureDependencyModel
{
    [Required] public string Code { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    public FeatureType? FeatureType { get; set; }

    [Required] public int TotalCount { get; set; }
    
    [Required] public int AutomatedCount { get; set; }
    
    [Required] public int ProblemCount { get; set; }
}
