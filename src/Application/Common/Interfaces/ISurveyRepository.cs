using NPS.Api.Domain.Entities;

namespace NPS.Api.Application.Common.Interfaces;

public interface ISurveyRepository
{
    Task AddResponseAsync(SurveyResponse response);
    Task<bool> HasUserVotedAsync(int userId);
    Task<IEnumerable<SurveyResponse>> GetAllResponsesAsync();
}
