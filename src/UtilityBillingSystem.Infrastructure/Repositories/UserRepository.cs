using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Infrastructure.Db;

namespace UtilityBillingSystem.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _factory;
    public UserRepository(IDbConnectionFactory factory) => _factory = factory;

    public UserRow? GetByUsername(string username)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
SELECT u.Id, u.FirstName, u.LastName, u.Username, u.PasswordHash, u.Role, u.CommunityId, u.IsActive
FROM dbo.Users u
WHERE u.Username = @u;";
        cmd.Parameters.AddWithValue("@u", username);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return new UserRow(
            Id: r.GetInt32(0),
            FirstName: r.GetString(1),
            LastName: r.GetString(2),
            Username: r.GetString(3),
            PasswordHash: r.GetString(4),
            Role: r.GetString(5),
            CommunityId: r.IsDBNull(6) ? null : r.GetInt32(6),
            IsActive: r.GetBoolean(7)
        );
    }

    public bool UsernameExists(string username)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE Username=@u) THEN 1 ELSE 0 END;";
        cmd.Parameters.AddWithValue("@u", username);
        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
    }

    public int CreateUser(string firstName, string lastName, string username, string passwordHash, string role, int? communityId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
INSERT INTO dbo.Users(FirstName, LastName, Username, PasswordHash, Role, CommunityId)
VALUES (@fn, @ln, @un, @ph, @role, @cid);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
        cmd.Parameters.AddWithValue("@fn", firstName);
        cmd.Parameters.AddWithValue("@ln", lastName);
        cmd.Parameters.AddWithValue("@un", username);
        cmd.Parameters.AddWithValue("@ph", passwordHash);
        cmd.Parameters.AddWithValue("@role", role);
        cmd.Parameters.AddWithValue("@cid", (object?)communityId ?? DBNull.Value);

        return (int)cmd.ExecuteScalar();
    }

    public void DeactivateUser(int userId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE dbo.Users SET IsActive = 0 WHERE Id=@id;";
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public void UpdateUserCommunity(int userId, int? communityId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = "UPDATE dbo.Users SET CommunityId = @cid WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.Parameters.AddWithValue("@cid", (object?)communityId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public List<UserListItem> GetAllUsers()
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
SELECT u.Id,
       u.Username,
       u.FirstName,
       u.LastName,
       u.Role,
       u.CommunityId,
       c.Name AS CommunityName,
       u.IsActive
FROM dbo.Users u
LEFT JOIN dbo.Communities c ON c.Id = u.CommunityId
ORDER BY u.Role, u.Username;";

        using var r = cmd.ExecuteReader();
        var list = new List<UserListItem>();
        while (r.Read())
        {
            list.Add(new UserListItem(
                Id: r.GetInt32(0),
                Username: r.GetString(1),
                FirstName: r.GetString(2),
                LastName: r.GetString(3),
                Role: r.GetString(4),
                CommunityId: r.IsDBNull(5) ? null : r.GetInt32(5),
                CommunityName: r.IsDBNull(6) ? null : r.GetString(6),
                IsActive: r.GetBoolean(7)
            ));
        }
        return list;
    }
}
