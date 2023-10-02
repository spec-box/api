using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class StructureModel
{
    [Required] public string Code { get; set; } = null!;
    [Required] public TreeNodeModel[] Tree { get; set; } = null!;
}
