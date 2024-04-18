using System.ComponentModel.DataAnnotations;
using SpecBox.WebApi.Model.Common;

namespace SpecBox.WebApi.Model.Project;

public class TreeGroupModel{

    [Required] public ProjectModel Project { get; set; } = null!;
    [Required] public TreeModel[] Trees { get; set; } = null!;
}
