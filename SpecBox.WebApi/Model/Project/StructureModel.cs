using System.ComponentModel.DataAnnotations;
using SpecBox.WebApi.Model.Common;

namespace SpecBox.WebApi.Model.Project;

public class StructureModel
{
    [Required] public ProjectModel Project { get; set; } = null!;
    [Required] public TreeNodeModel[] Tree { get; set; } = null!;
}
