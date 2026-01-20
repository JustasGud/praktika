namespace UtilityBillingSystem.Domain;

public sealed class Community
{
    public int Id { get; }
    public string Name { get; private set; }
    public string? Address { get; private set; }

    public Community(int id, string name, string? address)
    {
        Id = id;
        Name = name;
        Address = address;
    }

    public override string ToString() => Name;
}
