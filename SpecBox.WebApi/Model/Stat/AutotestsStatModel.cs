using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Stat;

public class AutotestsStatModel
{
    [Required] public DateTime Timestamp { get; set; }

    [Required] public int Duration { get; set; }

    [Required] public int AssertionsCount { get; set; }
}
