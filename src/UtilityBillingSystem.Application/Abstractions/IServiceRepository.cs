using UtilityBillingSystem.Domain;

namespace UtilityBillingSystem.Application.Abstractions;

public interface IServiceRepository
{
    int Create(string name, string? description);
    void Update(int id, string name, string? description, bool isActive);
    void Delete(int id);
    List<Service> GetAll(bool onlyActive = false);
    bool Any();
}
