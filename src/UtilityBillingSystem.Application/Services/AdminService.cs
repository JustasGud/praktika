using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Application.Security;
using UtilityBillingSystem.Domain;

namespace UtilityBillingSystem.Application.Services;

public sealed class AdminService
{
    private readonly IUserRepository _users;
    private readonly ICommunityRepository _communities;
    private readonly IServiceRepository _services;
    private readonly IPasswordHasher _hasher;

    public AdminService(IUserRepository users, ICommunityRepository communities, IServiceRepository services, IPasswordHasher hasher)
    {
        _users = users;
        _communities = communities;
        _services = services;
        _hasher = hasher;
    }

    // Auto prisijungimo generavimas (login = vardas, password = pavardė)
    public (string Username, string PlainPassword, int UserId) CreateUserGenerated(
        string firstName, string lastName, string role, int? communityId)
    {
        var baseUsername = firstName.Trim();
        if (string.IsNullOrWhiteSpace(baseUsername)) throw new InvalidOperationException("Vardas negali būti tuščias.");

        var username = baseUsername;
        var i = 0;
        while (_users.UsernameExists(username))
        {
            i++;
            username = $"{baseUsername}{i}";
        }

        var plainPassword = lastName.Trim();
        if (string.IsNullOrWhiteSpace(plainPassword)) throw new InvalidOperationException("Pavardė negali būti tuščia.");

        if (role == "RESIDENT" && communityId is null)
            throw new InvalidOperationException("Gyventojui būtina parinkti bendriją.");

        var hash = _hasher.Hash(plainPassword);
        var id = _users.CreateUser(firstName.Trim(), lastName.Trim(), username, hash, role, communityId);
        return (username, plainPassword, id);
    }

    public void DeactivateUser(int userId) => _users.DeactivateUser(userId);

    // NEW: admin can change user's community (bendrija)
    public void UpdateUserCommunity(int userId, int? communityId) => _users.UpdateUserCommunity(userId, communityId);

    public List<UserListItem> GetAllUsers() => _users.GetAllUsers();

    public int CreateCommunity(string name, string? address)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Bendrijos pavadinimas negali būti tuščias.");
        return _communities.Create(name.Trim(), address?.Trim());
    }

    public void UpdateCommunity(int id, string name, string? address)
    {
        if (id <= 0) throw new InvalidOperationException("Pasirink bendriją.");
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Pavadinimas negali būti tuščias.");
        _communities.Update(id, name.Trim(), address?.Trim());
    }

    public void DeleteCommunity(int id)
    {
        if (id <= 0) throw new InvalidOperationException("Pasirink bendriją.");
        _communities.Delete(id);
    }

    public List<Community> GetAllCommunities() => _communities.GetAll();

    public int CreateService(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Paslaugos pavadinimas negali būti tuščias.");
        return _services.Create(name.Trim(), description?.Trim());
    }

    public void UpdateService(int id, string name, string? description, bool isActive)
    {
        if (id <= 0) throw new InvalidOperationException("Pasirink paslaugą.");
        if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Pavadinimas negali būti tuščas.");
        _services.Update(id, name.Trim(), description?.Trim(), isActive);
    }

    public void DeleteService(int id)
    {
        if (id <= 0) throw new InvalidOperationException("Pasirink paslaugą.");
        _services.Delete(id);
    }

    public List<Service> GetAllServices(bool onlyActive = false) => _services.GetAll(onlyActive);

    public bool AnyCommunities() => _communities.Any();
    public bool AnyServices() => _services.Any();
}
