using Microsoft.Data.SqlClient;
using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Infrastructure.Db;

namespace UtilityBillingSystem.Infrastructure.Repositories;

public sealed class CommunityServiceRepository : ICommunityServiceRepository
{
    private readonly IDbConnectionFactory _factory;
    public CommunityServiceRepository(IDbConnectionFactory factory) => _factory = factory;

    public int AssignService(int communityId, int serviceId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.CommunityServices(CommunityId, ServiceId)
VALUES (@c, @s);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
        cmd.Parameters.AddWithValue("@c", communityId);
        cmd.Parameters.AddWithValue("@s", serviceId);

        try
        {
            return (int)cmd.ExecuteScalar();
        }
        catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
        {
            throw new InvalidOperationException("Ši paslauga jau priskirta pasirinktai bendrijai.");
        }
    }

    public int? GetCommunityServiceId(int communityId, int serviceId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT cs.Id FROM dbo.CommunityServices cs WHERE cs.CommunityId=@c AND cs.ServiceId=@s;";
        cmd.Parameters.AddWithValue("@c", communityId);
        cmd.Parameters.AddWithValue("@s", serviceId);

        var obj = cmd.ExecuteScalar();
        return obj is null || obj == DBNull.Value ? null : Convert.ToInt32(obj);
    }

    public List<AssignedServiceRow> GetAssignedServices(int communityId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
SELECT cs.Id, s.Id, s.Name
FROM dbo.CommunityServices cs
JOIN dbo.Services s ON s.Id = cs.ServiceId
WHERE cs.CommunityId = @c
ORDER BY s.Name;";
        cmd.Parameters.AddWithValue("@c", communityId);

        using var r = cmd.ExecuteReader();
        var list = new List<AssignedServiceRow>();
        while (r.Read())
        {
            list.Add(new AssignedServiceRow(
                CommunityServiceId: r.GetInt32(0),
                ServiceId: r.GetInt32(1),
                ServiceName: r.GetString(2)
            ));
        }
        return list;
    }
}
