using System.ComponentModel.DataAnnotations;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class TreeNodeModel: NodeModel
{ 
    public int? SortOrder { get; set; }
}
