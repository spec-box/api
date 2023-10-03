using AutoMapper;
using SpecBox.Domain.Model;
using SpecBox.WebApi.Model.Project;
using SpecBox.WebApi.Model.Stat;

namespace SpecBox.WebApi.Model;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<Assertion, AssertionModel>();
        CreateMap<AssertionGroup, AssertionGroupModel>();
        CreateMap<Feature, FeatureModel>();
        CreateMap<AutotestsStatRecord, AutotestsStatModel>();
        CreateMap<AssertionsStatRecord, AssertionsStatModel>();
        CreateMap<Domain.Model.Project, ProjectModel>();
    }
}
