namespace UtilityBillingSystem.Application.Abstractions;

public interface IUserRepository
{
    UserRow? GetByUsername(string username);
    bool UsernameExists(string username);

    int CreateUser(string firstName, string lastName, string username, string passwordHash, string role, int? communityId);
    void DeactivateUser(int userId);

    // Admin: allow changing user's community (bendrija)
    void UpdateUserCommunity(int userId, int? communityId);

    List<UserListItem> GetAllUsers();
}

public sealed record UserRow(
    int Id,
    string FirstName,
    string LastName,
    string Username,
    string PasswordHash,
    string Role,
    int? CommunityId,
    bool IsActive
);

public sealed record UserListItem(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    string Role,
    int? CommunityId,
    string? CommunityName,
    bool IsActive
)
{
    public string RoleLt => Role.Trim().ToUpperInvariant() switch
    {
        "ADMIN" => "Administratorius",
        "MANAGER" => "Vadybininkas",
        "RESIDENT" => "Gyventojas",
        _ => Role.Trim()
    };

    public string CommunityNameLt => string.IsNullOrWhiteSpace(CommunityName) ? "Bendrija nepasirinkta" : CommunityName!;
}
