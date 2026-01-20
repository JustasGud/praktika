using UtilityBillingSystem.Domain;

namespace UtilityBillingSystem.Application.Abstractions;

public interface ICommunityRepository
{
    int Create(string name, string? address);
    void Update(int id, string name, string? address);
    void Delete(int id);
    List<Community> GetAll();
    bool Any();
}
