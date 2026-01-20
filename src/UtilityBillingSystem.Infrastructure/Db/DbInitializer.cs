namespace UtilityBillingSystem.Infrastructure.Db;

public sealed class DbInitializer
{
    private readonly IDbConnectionFactory _factory;
    public DbInitializer(IDbConnectionFactory factory) => _factory = factory;

    public void EnsureDatabaseAndSchema()
    {
        EnsureDatabase();
        EnsureSchema();
    }

    private void EnsureDatabase()
    {
        using var con = _factory.CreateMaster();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
IF DB_ID(@db) IS NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE [' + @db + N']';
    EXEC sp_executesql @sql;
END";
        cmd.Parameters.AddWithValue("@db", _factory.DatabaseName);
        cmd.ExecuteNonQuery();
    }

    private void EnsureSchema()
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = SchemaSql.Value; // be GO
        cmd.ExecuteNonQuery();
    }

    public bool HasAnyUsers()
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users) THEN 1 ELSE 0 END;";
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }
}
