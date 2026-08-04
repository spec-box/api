using Microsoft.AspNetCore.Mvc;
using SpecBox.Domain.Model.Enums;

namespace SpecBox.WebApi.Model.Project;

public class StructureFilterModel
{
    /// <summary>
    /// Подстрока названия узла, без учёта регистра.
    /// Если совпало название группирующего узла, фильтр выполняется и для всех его потомков.
    /// </summary>
    [FromQuery(Name = "title")]
    public string? Title { get; set; }

    /// <summary>Тип фичи; групповые узлы фильтру не удовлетворяют.</summary>
    [FromQuery(Name = "featureType")]
    public FeatureType? FeatureType { get; set; }

    /// <summary>true — только узлы с проблемными проверками, false — только без них.</summary>
    [FromQuery(Name = "hasProblems")]
    public bool? HasProblems { get; set; }
}
