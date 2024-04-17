using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class FeatureDependencyModel
{
    [Required] public string Code { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    public FeatureType? FeatureType { get; set; }

    public int AssertionsCount { get; set; }
    
    public int AutomatedCount { get; set; }
}