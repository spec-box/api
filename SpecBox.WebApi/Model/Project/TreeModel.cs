using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class TreeModel
{
  [Required] public string Code { get; set; } = null!;
  [Required] public string Title { get; set; } = null!;
}
