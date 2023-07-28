using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Upload;

public class AssertionModel
{
    [Required] public string Title { get; set; } = null!;
    public string? Description { get; set; }
    [Required] public bool IsAutomated { get; set; }
}
