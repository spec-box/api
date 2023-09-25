using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("Export")]
public class Export
{
    public Guid Id { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public Project Project { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
