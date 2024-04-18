using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class TreeModel
{
    [Required] public required string Code { get; set; }

    [Required] public string? Title { get; set; }

}
