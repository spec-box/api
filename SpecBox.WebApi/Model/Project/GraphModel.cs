using System.ComponentModel.DataAnnotations;
using SpecBox.WebApi.Model.Common;

namespace SpecBox.WebApi.Model.Project;

public class GraphModel
{
    [Required] public NodeModel[] Nodes { get; set; } = null!;
    [Required] public EdgeModel[] Edges { get; set; } = null!;
}
