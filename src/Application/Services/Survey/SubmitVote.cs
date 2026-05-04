using FluentValidation;
using MediatR;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Application.Common.Models;
using NPS.Api.Application.Common.Helpers;
using NPS.Api.Domain.Entities;

namespace NPS.Api.Application.Services.Survey;

// Body: { score }. El usuario sale del JWT — sin userId en el request a propósito.
public record CreateSurveyResponseCommand(int Score) : IRequest<BaseResponseDto>;

public class CreateSurveyResponseValidator : AbstractValidator<CreateSurveyResponseCommand>
{
    public CreateSurveyResponseValidator()
    {
        // 0–10 cerrado; fuera de eso FluentValidation frenó la request antes del handler.
        RuleFor(x => x.Score)
            .InclusiveBetween(0, 10)
            .WithMessage("El puntaje debe estar entre 0 y 10.");
    }
}

public class CreateSurveyResponseHandler : IRequestHandler<CreateSurveyResponseCommand, BaseResponseDto>
{
    private readonly ISurveyRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CreateSurveyResponseHandler(ISurveyRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<BaseResponseDto> Handle(CreateSurveyResponseCommand request, CancellationToken ct)
    {
        var response = new BaseResponseDto();

        try
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Usuario no identificado");

            // Un solo voto por persona para no viciar el NPS
            if (await _repository.HasUserVotedAsync(userId))
            {
                return BaseResponseDto.Failure("Ya has participado en esta encuesta. ¡Gracias!", 400);
            }

            var survey = new SurveyResponse
            {
                UserId = userId,
                Score = request.Score,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddResponseAsync(survey);

            return BaseResponseDto.Success("Voto registrado correctamente", 201);
        }
        catch (Exception ex)
        {
            ServiceResponseBuilder.ApplyUnexpectedError(response, ex);
            return response;
        }
    }
}
