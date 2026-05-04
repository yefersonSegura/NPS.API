using AutoMapper;
using MediatR;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Application.Common.Models;
using NPS.Api.Application.Common.Helpers;
using NPS.Api.Application.Common.Nps;

namespace NPS.Api.Application.Services.Survey;

public record GetNpsResultsQuery : IRequest<ResponseDto<NpsResultsDto>>;

public record NpsResultsDto(
    double NpsScore,
    double PromotersPercentage,
    double DetractorsPercentage,
    double PassivesPercentage,
    int TotalResponses);

public class GetNpsResultsHandler : IRequestHandler<GetNpsResultsQuery, ResponseDto<NpsResultsDto>>
{
    private readonly ISurveyRepository _repository;
    private readonly IMapper _mapper;

    public GetNpsResultsHandler(ISurveyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseDto<NpsResultsDto>> Handle(GetNpsResultsQuery request, CancellationToken ct)
    {
        var response = new ResponseDto<NpsResultsDto>();

        try
        {
            // No filtramos por tenant: el ejercicio es una sola encuesta global.
            var data = (await _repository.GetAllResponsesAsync()).ToList();
            var scores = data.Select(x => x.Score).ToList();
            var computed = NpsCalculator.Compute(scores);

            if (computed.TotalResponses == 0)
            {
                return ResponseDto<NpsResultsDto>.Success(
                    _mapper.Map<NpsResultsDto>(NpsComputationResult.Empty),
                    "No hay respuestas registradas aún");
            }

            var dto = _mapper.Map<NpsResultsDto>(computed);
            return ResponseDto<NpsResultsDto>.Success(dto, "Cálculo de NPS finalizado");
        }
        catch (Exception ex)
        {
            ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
            response.Data = null;
            return response;
        }
    }
}
