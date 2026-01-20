namespace UtilityBillingSystem.Domain.Users;

public abstract class User
{
    public int Id { get; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Username { get; private set; }
    public UserRole Role { get; }
    public bool IsActive { get; private set; }

    protected User(int id, string firstName, string lastName, string username, UserRole role, bool isActive)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Role = role;
        IsActive = isActive;
    }

    // Polimorfizmas: tas pats metodas, bet skirtingas elgesys
    public abstract IReadOnlyList<string> GetMenuItems();

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
