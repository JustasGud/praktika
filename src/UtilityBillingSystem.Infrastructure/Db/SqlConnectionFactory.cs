using Microsoft.Data.SqlClient;

namespace UtilityBillingSystem.Infrastructure.Db;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    public string DatabaseName { get; }

    private readonly string _masterCs =
        "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true;";

    private readonly string _appCs;

    public SqlConnectionFactory(string databaseName)
    {
        DatabaseName = databaseName;
        _appCs = $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;";
    }

    public SqlConnection CreateMaster() => new SqlConnection(_masterCs);
    public SqlConnection CreateAppDb() => new SqlConnection(_appCs);
}
