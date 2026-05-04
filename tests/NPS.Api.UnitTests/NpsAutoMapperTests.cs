using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NPS.Api.Application.Common.Mappings;
using NPS.Api.Application.Common.Nps;
using NPS.Api.Application.Services.Survey;

namespace NPS.Api.UnitTests;

public sealed class NpsAutoMapperTests
{
    [Fact]
    public void Maps_computation_result_to_api_dto()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(_ => { }, typeof(NpsResultsProfile).Assembly);
        using var provider = services.BuildServiceProvider();

        var mapper = provider.GetRequiredService<IMapper>();

        var source = new NpsComputationResult(-100, 0, 100, 0, 1);
        var dto = mapper.Map<NpsResultsDto>(source);

        Assert.Equal(-100, dto.NpsScore);
        Assert.Equal(0, dto.PromotersPercentage);
        Assert.Equal(100, dto.DetractorsPercentage);
        Assert.Equal(0, dto.PassivesPercentage);
        Assert.Equal(1, dto.TotalResponses);
    }
}
