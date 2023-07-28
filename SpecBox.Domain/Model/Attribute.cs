using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("Attribute")]
public class Attribute
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
