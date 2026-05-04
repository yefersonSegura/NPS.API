using Dapper;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Domain.Entities;
using NPS.Api.Infrastructure.Persistence.Common;

namespace NPS.Api.Infrastructure.Persistence;

public class SurveyRepository : BaseRepository, ISurveyRepository
{
    public SurveyRepository(ISqlConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public async Task AddResponseAsync(SurveyResponse response)
    {
        using var connection = GetConnection();
        const string sql =
            "INSERT INTO SurveyResponses (UserId, Score, CreatedAt) VALUES (@UserId, @Score, @CreatedAt)";
        await connection.ExecuteAsync(sql, response);
    }

    public async Task<bool> HasUserVotedAsync(int userId)
    {
        using var connection = GetConnection();
        const string sql = "SELECT COUNT(1) FROM SurveyResponses WHERE UserId = @UserId";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
        return count > 0;
    }

    public async Task<IEnumerable<SurveyResponse>> GetAllResponsesAsync()
    {
        using var connection = GetConnection();
        return await connection.QueryAsync<SurveyResponse>("SELECT * FROM SurveyResponses");
    }
}
