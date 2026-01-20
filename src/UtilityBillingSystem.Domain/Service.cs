namespace UtilityBillingSystem.Domain;

public sealed class Service
{
    public int Id { get; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public Service(int id, string name, string? description, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    public override string ToString() => Name;
}
