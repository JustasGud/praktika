using System;
using System.Windows;
using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Application.Auth;
using UtilityBillingSystem.Application.Security;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.Infrastructure.Db;
using UtilityBillingSystem.Infrastructure.Repositories;
using UtilityBillingSystem.UI.Wpf.Views;

namespace UtilityBillingSystem.UI.Wpf;

public partial class App : System.Windows.Application
{
    private const string DatabaseName = "UtilityBillingSystemDb";

    // Išsaugome serviso objektus, kad galėtume atidaryti prisijungimo langą ir po atsijungimo.
    private AuthService? _auth;
    private AdminService? _adminService;
    private ManagerService? _managerService;
    private ResidentService? _residentService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Composition root (be DI bibliotekų)
        var factory = new SqlConnectionFactory(DatabaseName);
        var dbInit = new DbInitializer(factory);
        dbInit.EnsureDatabaseAndSchema();

        IPasswordHasher hasher = new PasswordHasher();

        IUserRepository userRepo = new UserRepository(factory);
        ICommunityRepository communityRepo = new CommunityRepository(factory);
        IServiceRepository serviceRepo = new ServiceRepository(factory);
        ICommunityServiceRepository csRepo = new CommunityServiceRepository(factory);
        IPricingRepository pricingRepo = new PricingRepository(factory);

        // Minimalus seed: admin + bent 1 bendrija ir 2 paslaugos (patogiam testui)
        if (!dbInit.HasAnyUsers())
        {
            var adminHash = hasher.Hash("admin");
            userRepo.CreateUser("Admin", "Admin", "admin", adminHash, "ADMIN", null);
        }

        var adminService = new AdminService(userRepo, communityRepo, serviceRepo, hasher);

        if (!adminService.AnyCommunities())
            adminService.CreateCommunity("Bendrija A", "Vilnius");

        if (!adminService.AnyServices())
        {
            adminService.CreateService("Šildymas", "Mėnesinis šildymo mokestis");
            adminService.CreateService("Vanduo", "Šalto vandens mokestis");
        }

        _auth = new AuthService(userRepo, hasher);
        _adminService = adminService;
        _managerService = new ManagerService(csRepo, pricingRepo);
        _residentService = new ResidentService(pricingRepo);

        ShowLoginWindow();
    }

    public void ShowLoginWindow()
    {
        // Apsauga, jei metodas būtų pakviestas per anksti.
        if (_auth is null || _adminService is null || _managerService is null || _residentService is null)
            throw new InvalidOperationException("Programos servisai neinicijuoti.");

        var login = new LoginWindow(_auth, _adminService, _managerService, _residentService);
        login.Show();
    }
}
