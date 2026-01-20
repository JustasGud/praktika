namespace UtilityBillingSystem.Domain.Users;

public sealed class Resident : User
{
    public int CommunityId { get; }

    public Resident(int id, string fn, string ln, string un, bool active, int communityId)
        : base(id, fn, ln, un, UserRole.Resident, active)
    {
        CommunityId = communityId;
    }

    public override IReadOnlyList<string> GetMenuItems()
        => new[] { "Mano paslaugos" };
}
