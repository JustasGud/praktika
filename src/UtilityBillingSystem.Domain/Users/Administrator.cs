namespace UtilityBillingSystem.Domain.Users;

public sealed class Administrator : User
{
    public Administrator(int id, string fn, string ln, string un, bool active)
        : base(id, fn, ln, un, UserRole.Admin, active) { }

    public override IReadOnlyList<string> GetMenuItems()
        => new[]
        {
        "Bendrijos",
        "Paslaugos",
        "Vartotojai",
        "Priskyrimas bendrijoms",
        "Kainų valdymas",
        "Mano paslaugos"
        };
}
