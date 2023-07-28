using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Upload;

public class AssertionGroupModel
{
    [Required] public string Title { get; set; } = null!;

    [Required] public AssertionModel[] Assertions { get; set; } = null!;
}
