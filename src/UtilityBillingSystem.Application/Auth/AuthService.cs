using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Application.Security;
using UtilityBillingSystem.Domain.Users;

namespace UtilityBillingSystem.Application.Auth;

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public AuthService(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public User Login(string username, string password)
    {
        var row = _users.GetByUsername(username);
        if (row is null) throw new InvalidOperationException("Neteisingas prisijungimo vardas arba slaptažodis.");
        if (!row.IsActive) throw new InvalidOperationException("Vartotojas neaktyvus.");
        if (!_hasher.Verify(password, row.PasswordHash))
            throw new InvalidOperationException("Neteisingas prisijungimo vardas arba slaptažodis.");

        return row.Role switch
        {
            "ADMIN" => new Administrator(row.Id, row.FirstName, row.LastName, row.Username, row.IsActive),
            "MANAGER" => new Manager(row.Id, row.FirstName, row.LastName, row.Username, row.IsActive),
            "RESIDENT" => new Resident(row.Id, row.FirstName, row.LastName, row.Username, row.IsActive,
                                       row.CommunityId ?? throw new InvalidOperationException("Gyventojas neturi priskirtos bendrijos.")),
            _ => throw new InvalidOperationException("Nežinoma rolė DB įraše.")
        };
    }
}
