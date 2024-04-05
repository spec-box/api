using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Project;

public class AssertionGroupModel
{
    [Required] public string Title { get; set; } = null!;
    
    public int? SortOrder { get; set; }

    [Required] public List<AssertionModel> Assertions { get; } = new();
}
