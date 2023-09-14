using System.ComponentModel.DataAnnotations;

namespace SpecBox.WebApi.Model.Upload;

public class UploadData
{
    [Required] public FeatureModel[] Features { get; set; } = null!;

    [Required] public AttributeModel[] Attributes { get; set; } = null!;

    public TreeModel[] Trees { get; set; } = null!;
}
