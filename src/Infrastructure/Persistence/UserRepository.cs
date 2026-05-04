using Dapper;
using NPS.Api.Application.Common.Interfaces;
using NPS.Api.Domain.Entities;
using NPS.Api.Infrastructure.Persistence.Common;

namespace NPS.Api.Infrastructure.Persistence;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(ISqlConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username",
            new { Username = username });
    }

    public async Task UpdateAsync(User user)
    {
        using var connection = GetConnection();
        const string sql = @"
            UPDATE Users 
            SET FailedAttempts = @FailedAttempts, 
                LockoutEnd = @LockoutEnd, 
                RefreshToken = @RefreshToken, 
                RefreshTokenExpiry = @RefreshTokenExpiry 
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE RefreshToken = @RefreshToken",
            new { RefreshToken = refreshToken });
    }
}
