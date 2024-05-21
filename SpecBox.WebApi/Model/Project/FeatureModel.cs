using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class FeatureModel
{
    [Required] public string Code { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    public FeatureType? FeatureType { get; set; }

    public string? Description { get; set; }

    public string? FilePath { get; set; }
    
    [Required] public List<RelatedFeatureModel> Usages { get; set; } = new();

    [Required] public List<AssertionGroupModel> AssertionGroups { get; } = new();
}
