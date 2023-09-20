using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("AutotestsStat")]
public class AutotestsStatRecord
{
    public Guid Id { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public Project Project { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public int Duration { get; set; }
    
    public int AssertionsCount { get; set; }
}
