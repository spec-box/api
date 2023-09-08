using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class TreeNodeModel
{
    [Required] public string Id { get; set; } = null!;

    [Required] public string[] Path { get; set; } = null!;

    public string? ParentId { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;

    [Required] public int CoverageRate { get; set; }
}
