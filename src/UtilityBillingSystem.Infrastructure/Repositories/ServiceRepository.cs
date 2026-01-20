using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Domain;
using UtilityBillingSystem.Infrastructure.Db;

namespace UtilityBillingSystem.Infrastructure.Repositories;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly IDbConnectionFactory _factory;
    public ServiceRepository(IDbConnectionFactory factory) => _factory = factory;

    public bool Any()
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Services) THEN 1 ELSE 0 END;";
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public int Create(string name, string? description)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.Services(Name, Description) VALUES (@n, @d);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@d", (object?)description ?? DBNull.Value);
        return (int)cmd.ExecuteScalar();
    }

    public void Update(int id, string name, string? description, bool isActive)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE dbo.Services SET Name=@n, Description=@d, IsActive=@a WHERE Id=@id;";
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@d", (object?)description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@a", isActive);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var tx = con.BeginTransaction();
        try
        {
            // 1) Delete prices for all community-service links that reference this service
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
DELETE p
FROM dbo.Prices p
WHERE p.CommunityServiceId IN (
    SELECT cs.Id
    FROM dbo.CommunityServices cs
    WHERE cs.ServiceId = @id
);";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // 2) Delete community-service assignments for this service
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM dbo.CommunityServices WHERE ServiceId = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            // 3) Delete the service itself
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM dbo.Services WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }


    public List<Service> GetAll(bool onlyActive = false)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = onlyActive
            ? "SELECT Id, Name, Description, IsActive FROM dbo.Services WHERE IsActive=1 ORDER BY Name;"
            : "SELECT Id, Name, Description, IsActive FROM dbo.Services ORDER BY Name;";

        using var r = cmd.ExecuteReader();
        var list = new List<Service>();
        while (r.Read())
        {
            list.Add(new Service(
                id: r.GetInt32(0),
                name: r.GetString(1),
                description: r.IsDBNull(2) ? null : r.GetString(2),
                isActive: r.GetBoolean(3)
            ));
        }
        return list;
    }
}
