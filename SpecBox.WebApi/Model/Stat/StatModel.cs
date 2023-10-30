using System.ComponentModel.DataAnnotations;
using SpecBox.WebApi.Model.Common;

namespace SpecBox.WebApi.Model.Stat;

public class StatModel
{
    [Required] public ProjectModel Project { get; set; } = null!;

    [Required] public AssertionsStatModel[] Assertions { get; set; } = null!;

    [Required] public AutotestsStatModel[] Autotests { get; set; } = null!;
}
