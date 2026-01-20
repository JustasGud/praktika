using Microsoft.Data.SqlClient;

namespace UtilityBillingSystem.Infrastructure.Db;

public interface IDbConnectionFactory
{
    string DatabaseName { get; }
    SqlConnection CreateMaster();
    SqlConnection CreateAppDb();
}
