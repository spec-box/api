using System.ComponentModel.DataAnnotations.Schema;

namespace SpecBox.Domain.Model;

[Table("AssertionsStat")]
public class AssertionsStatRecord
{
    public Guid Id { get; set; }
    
    public Guid ProjectId { get; set; }
    
    public Project Project { get; set; } = null!;

    public DateTime Timestamp { get; set; }
    
    public int TotalCount { get; set; }
    
    public int AutomatedCount { get; set; }
}
