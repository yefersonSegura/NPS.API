using AutoMapper;
using NPS.Api.Application.Common.Nps;
using NPS.Api.Application.Services.Survey;

namespace NPS.Api.Application.Common.Mappings;

public class NpsResultsProfile : Profile
{
    public NpsResultsProfile()
    {
        CreateMap<NpsComputationResult, NpsResultsDto>();
    }
}
