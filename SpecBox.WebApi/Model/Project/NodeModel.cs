using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class NodeModel
{
    [Required] public Guid Id { get; set; }

    [Required] public string FeatureCode { get; set; } = null!;
    
    public FeatureType? FeatureType { get; set; }

    [Required] public string Title { get; set; } = null!;

    [Required] public int TotalCount { get; set; }

    [Required] public int AutomatedCount { get; set; }
    [Required] public int ProblemCount { get; set; }
}