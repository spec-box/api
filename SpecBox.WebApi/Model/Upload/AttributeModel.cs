using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Upload;

public class AttributeModel
{
    [Required] public string Code { get; set; } = null!;

    [Required] public string Title { get; set; } = null!;
    
    [Required] public AttributeValueModel[] Values { get; set; } = null!;
}
