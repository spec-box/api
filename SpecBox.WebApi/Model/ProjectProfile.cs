using AutoMapper;
using SpecBox.Domain.Model;
using SpecBox.WebApi.Model.Project;

namespace SpecBox.WebApi.Model;

public class ProjectProfile: Profile
{
    public ProjectProfile()
    {
        CreateMap<Assertion, AssertionModel>();
        CreateMap<AssertionGroup, AssertionGroupModel>();
        CreateMap<Feature, FeatureModel>();
    }
}
