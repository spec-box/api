using System.ComponentModel.DataAnnotations.Schema;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.Domain.Model;

[Table("FeatureDependency")]
public class FeatureDependency
{
    public Guid SourceFeatureId { get; set; }
    
    public Guid DependencyFeatureId { get; set; }

    public Feature SourceFeature { get; set; }

    public Feature DependencyFeature { get; set; }
}
