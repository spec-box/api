using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Upload;

public class UploadData
{
    public string? Message { get; set; }
    
    [Required] public FeatureModel[] Features { get; set; } = null!;

    [Required] public AttributeModel[] Attributes { get; set; } = null!;

    [Required] public TreeModel[] Trees { get; set; } = null!;
}
