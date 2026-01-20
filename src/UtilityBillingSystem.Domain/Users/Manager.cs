namespace UtilityBillingSystem.Domain.Users;

public sealed class Manager : User
{
    public Manager(int id, string fn, string ln, string un, bool active)
        : base(id, fn, ln, un, UserRole.Manager, active) { }


    public override IReadOnlyList<string> GetMenuItems()
       => new[]
       {
        "Priskyrimas bendrijoms",
        "Kainų valdymas",
        "Mano paslaugos"
       };
}
