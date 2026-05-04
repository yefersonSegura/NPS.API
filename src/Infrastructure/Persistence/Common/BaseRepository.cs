using System.Data;
using NPS.Api.Application.Common.Interfaces;

namespace NPS.Api.Infrastructure.Persistence.Common;

public abstract class BaseRepository
{
    protected readonly ISqlConnectionFactory DbConnectionFactory;

    protected BaseRepository(ISqlConnectionFactory dbConnectionFactory)
    {
        DbConnectionFactory = dbConnectionFactory;
    }

    protected IDbConnection GetConnection() => DbConnectionFactory.CreateConnection();
}
