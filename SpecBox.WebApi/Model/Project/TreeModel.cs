using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class TreeModel
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;

}
