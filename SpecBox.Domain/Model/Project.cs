using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("Project")]
public class Project
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
