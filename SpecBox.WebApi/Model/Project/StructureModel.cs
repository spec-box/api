using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class StructureModel
{
    [Required] public TreeNodeModel[] Tree { get; set; } = null!;
}
