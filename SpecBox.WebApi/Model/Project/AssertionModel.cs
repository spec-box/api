using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class AssertionModel
{
    [Required] public string Title { get; set; } = null!;
    public string? Description { get; set; }
    [Required] public bool IsAutomated { get; set; }
    [Required] public AutomationState AutomationState { get; set; }
}
