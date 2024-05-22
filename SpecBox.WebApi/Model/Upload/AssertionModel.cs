using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Upload;

public class AssertionModel
{
    [Required] public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? DetailsUrl { get; set; }
    public bool? IsAutomated { get; set; }
    public AutomationState? AutomationState { get; set; }
}
