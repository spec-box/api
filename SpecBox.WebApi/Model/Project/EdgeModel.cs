using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class EdgeModel
{
    [Required] public Guid SourceId { get; set; }
    [Required] public Guid TargetId { get; set; }
}