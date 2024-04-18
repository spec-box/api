using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.Model;

[Table("FeatureDependency")]
[PrimaryKey("SourceFeatureId", "DependencyFeatureId")]
public class FeatureDependency
{
    public Guid SourceFeatureId { get; set; }
    
    public Guid DependencyFeatureId { get; set; }

    public Feature? SourceFeature { get; set; }

    public Feature? DependencyFeature { get; set; }
}
