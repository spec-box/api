using System.ComponentModel.DataAnnotations.Schema;

namespace tms.Domain.Model;

[Table("Entity")]
public class Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
}
