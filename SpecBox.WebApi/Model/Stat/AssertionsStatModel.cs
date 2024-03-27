using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Stat;

public class AssertionsStatModel
{
    [Required] public DateTime Timestamp { get; set; }

    [Required] public int TotalCount { get; set; }

    [Required] public int AutomatedCount { get; set; }
    
    [Required] public int ProblemCount { get; set; }
}
