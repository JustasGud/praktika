using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Domain;
using UtilityBillingSystem.Infrastructure.Db;

namespace UtilityBillingSystem.Infrastructure.Repositories;

public sealed class CommunityRepository : ICommunityRepository
{
    private readonly IDbConnectionFactory _factory;
    public CommunityRepository(IDbConnectionFactory factory) => _factory = factory;

    public bool Any()
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Communities) THEN 1 ELSE 0 END;";
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public int Create(string name, string? address)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.Communities(Name, Address) VALUES (@n, @a);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@a", (object?)address ?? DBNull.Value);
        return (int)cmd.ExecuteScalar();
    }

    public void Update(int id, string name, string? address)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE dbo.Communities SET Name=@n, Address=@a WHERE Id=@id;";
        cmd.Parameters.AddWithValue("@n", name);
        cmd.Parameters.AddWithValue("@a", (object?)address ?? DBNull.Value);
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
            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "UPDATE dbo.Users SET CommunityId = NULL WHERE CommunityId = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = con.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "DELETE FROM dbo.Communities WHERE Id = @id";
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

    public List<Community> GetAll()
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Address FROM dbo.Communities ORDER BY Name;";

        using var r = cmd.ExecuteReader();
        var list = new List<Community>();
        while (r.Read())
        {
            list.Add(new Community(
                id: r.GetInt32(0),
                name: r.GetString(1),
                address: r.IsDBNull(2) ? null : r.GetString(2)
            ));
        }
        return list;
    }
}
