using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Data.SqlClient;
using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.Domain;
using UtilityBillingSystem.Infrastructure.Db;
using UtilityBillingSystem.UI.Wpf.Mvvm;

namespace UtilityBillingSystem.UI.Wpf.ViewModels;

public sealed class RoleOption
{
    public string Value { get; init; } = "";   // internal: "ADMIN" / "MANAGER" / "RESIDENT"
    public string Display { get; init; } = ""; // UI: Lithuanian
    public override string ToString() => Display;
}

public sealed class AdminViewModel : ViewModelBase
{
    private readonly AdminService _admin;

    public ObservableCollection<Community> Communities { get; } = new();
    public ObservableCollection<Service> Services { get; } = new();
    public ObservableCollection<UserListItem> Users { get; } = new();

    // For ComboBoxes where we want to allow "no community"
    public ObservableCollection<Community> CommunityChoices { get; } = new();

    // Role dropdown: show LT text, store internal value
    public IReadOnlyList<RoleOption> RoleOptions { get; } = new[]
    {
        new RoleOption { Value = "ADMIN", Display = "Administratorius" },
        new RoleOption { Value = "MANAGER", Display = "Vadybininkas" },
        new RoleOption { Value = "RESIDENT", Display = "Gyventojas" }
    };

    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }

    // UI navigation (tabs)
    private int _selectedTabIndex;
    public int SelectedTabIndex { get => _selectedTabIndex; set => Set(ref _selectedTabIndex, value); }

    // Communities form
    private string _communityName = "";
    public string CommunityName { get => _communityName; set => Set(ref _communityName, value); }

    private string _communityAddress = "";
    public string CommunityAddress { get => _communityAddress; set => Set(ref _communityAddress, value); }

    private Community? _selectedCommunity;
    public Community? SelectedCommunity
    {
        get => _selectedCommunity;
        set
        {
            if (Set(ref _selectedCommunity, value) && value is not null)
            {
                CommunityName = value.Name;
                CommunityAddress = value.Address ?? "";
            }
        }
    }

    // Services form
    private string _serviceName = "";
    public string ServiceName { get => _serviceName; set => Set(ref _serviceName, value); }

    private string _serviceDescription = "";
    public string ServiceDescription { get => _serviceDescription; set => Set(ref _serviceDescription, value); }

    private bool _serviceIsActive = true;
    public bool ServiceIsActive { get => _serviceIsActive; set => Set(ref _serviceIsActive, value); }

    private Service? _selectedService;
    public Service? SelectedService
    {
        get => _selectedService;
        set
        {
            if (Set(ref _selectedService, value) && value is not null)
            {
                ServiceName = value.Name;
                ServiceDescription = value.Description ?? "";
                ServiceIsActive = value.IsActive;
            }
        }
    }

    // Users form (create)
    private string _userFirstName = "";
    public string UserFirstName { get => _userFirstName; set => Set(ref _userFirstName, value); }

    private string _userLastName = "";
    public string UserLastName { get => _userLastName; set => Set(ref _userLastName, value); }

    private string _userRole = "RESIDENT";
    public string UserRole { get => _userRole; set => Set(ref _userRole, value); }

    private Community? _userCommunity;
    public Community? UserCommunity { get => _userCommunity; set => Set(ref _userCommunity, value); }

    // Users form (selected user management)
    private UserListItem? _selectedUser;
    public UserListItem? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (Set(ref _selectedUser, value))
            {
                // Preselect their current community in the "change community" dropdown
                if (value?.CommunityId is int cid)
                    SelectedUserCommunity = CommunityChoices.FirstOrDefault(c => c.Id == cid) ?? CommunityChoices.FirstOrDefault(c => c.Id == 0);
                else
                    SelectedUserCommunity = CommunityChoices.FirstOrDefault(c => c.Id == 0);
            }
        }
    }

    private Community? _selectedUserCommunity;
    public Community? SelectedUserCommunity { get => _selectedUserCommunity; set => Set(ref _selectedUserCommunity, value); }

    // Commands
    public RelayCommand AddCommunityCommand { get; }
    public RelayCommand UpdateCommunityCommand { get; }
    public RelayCommand DeleteCommunityCommand { get; }

    public RelayCommand AddServiceCommand { get; }
    public RelayCommand UpdateServiceCommand { get; }
    public RelayCommand DeleteServiceCommand { get; }

    public RelayCommand CreateUserCommand { get; }
    public RelayCommand DeactivateUserCommand { get; }
    public RelayCommand UpdateUserCommunityCommand { get; }

    public AdminViewModel(AdminService admin)
    {
        _admin = admin;

        AddCommunityCommand = new RelayCommand(AddCommunity);
        UpdateCommunityCommand = new RelayCommand(UpdateCommunity);
        DeleteCommunityCommand = new RelayCommand(DeleteCommunity);

        AddServiceCommand = new RelayCommand(AddService);
        UpdateServiceCommand = new RelayCommand(UpdateService);
        DeleteServiceCommand = new RelayCommand(DeleteService);

        CreateUserCommand = new RelayCommand(CreateUser);
        DeactivateUserCommand = new RelayCommand(DeactivateUser);
        UpdateUserCommunityCommand = new RelayCommand(UpdateUserCommunity);

        RefreshAll();
    }

    private void RefreshAll()
    {
        ErrorMessage = "";
        LoadCommunities();
        LoadServices();
        LoadUsers();

        // Defaults for create-user form
        UserCommunity = CommunityChoices.FirstOrDefault(c => c.Id == 0);
    }

    private void LoadCommunities()
    {
        Communities.Clear();
        foreach (var c in _admin.GetAllCommunities())
            Communities.Add(c);

        // Build combo choices (with a placeholder for "none")
        CommunityChoices.Clear();
        CommunityChoices.Add(new Community(0, "— Bendrija nepasirinkta —", null));
        foreach (var c in Communities)
            CommunityChoices.Add(c);

        // Keep selection if possible
        if (UserCommunity is not null)
            UserCommunity = CommunityChoices.FirstOrDefault(x => x.Id == UserCommunity.Id) ?? CommunityChoices.FirstOrDefault(x => x.Id == 0);

        if (SelectedUserCommunity is not null)
            SelectedUserCommunity = CommunityChoices.FirstOrDefault(x => x.Id == SelectedUserCommunity.Id) ?? CommunityChoices.FirstOrDefault(x => x.Id == 0);
    }

    private void LoadServices()
    {
        Services.Clear();
        foreach (var s in _admin.GetAllServices())
            Services.Add(s);
    }

    private void LoadUsers()
    {
        Users.Clear();
        foreach (var u in _admin.GetAllUsers())
            Users.Add(u);
    }

    private void AddCommunity()
    {
        try
        {
            ErrorMessage = "";
            _admin.CreateCommunity(CommunityName, CommunityAddress);
            CommunityName = "";
            CommunityAddress = "";
            LoadCommunities();
        }
        catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
        {
            ErrorMessage = "Tokia bendrija jau egzistuoja.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void UpdateCommunity()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedCommunity is null) throw new InvalidOperationException("Pasirink bendriją.");
            _admin.UpdateCommunity(SelectedCommunity.Id, CommunityName, CommunityAddress);
            LoadCommunities();
        }
        catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
        {
            ErrorMessage = "Toks bendrijos pavadinimas jau naudojamas.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void DeleteCommunity()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedCommunity is null) throw new InvalidOperationException("Pasirink bendriją.");
            _admin.DeleteCommunity(SelectedCommunity.Id);
            SelectedCommunity = null;
            CommunityName = "";
            CommunityAddress = "";
            LoadCommunities();
            LoadUsers(); // gali pasikeisti (FK)
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void AddService()
    {
        try
        {
            ErrorMessage = "";
            _admin.CreateService(ServiceName, ServiceDescription);
            ServiceName = "";
            ServiceDescription = "";
            ServiceIsActive = true;
            LoadServices();
        }
        catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
        {
            ErrorMessage = "Tokia paslauga jau egzistuoja.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void UpdateService()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedService is null) throw new InvalidOperationException("Pasirink paslaugą.");
            _admin.UpdateService(SelectedService.Id, ServiceName, ServiceDescription, ServiceIsActive);
            LoadServices();
        }
        catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
        {
            ErrorMessage = "Toks paslaugos pavadinimas jau naudojamas.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void DeleteService()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedService is null) throw new InvalidOperationException("Pasirink paslaugą.");
            _admin.DeleteService(SelectedService.Id);
            SelectedService = null;
            ServiceName = "";
            ServiceDescription = "";
            ServiceIsActive = true;
            LoadServices();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void CreateUser()
    {
        try
        {
            ErrorMessage = "";

            int? communityId = (UserCommunity is null || UserCommunity.Id == 0) ? null : UserCommunity.Id;
            var (username, password, _) = _admin.CreateUserGenerated(UserFirstName, UserLastName, UserRole, communityId);

            LoadUsers();

            MessageBox.Show(
                $"Sukurta!\nUsername: {username}\nPassword: {password}",
                "Prisijungimo duomenys",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            UserFirstName = "";
            UserLastName = "";
            UserCommunity = CommunityChoices.FirstOrDefault(c => c.Id == 0);
        }
        catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
        {
            ErrorMessage = "Nepavyko sukurti: unikalumo pažeidimas (gal užimtas username ar pan.).";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void DeactivateUser()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedUser is null) throw new InvalidOperationException("Pasirink vartotoją.");
            _admin.DeactivateUser(SelectedUser.Id);
            LoadUsers();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void UpdateUserCommunity()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedUser is null) throw new InvalidOperationException("Pasirink vartotoją.");

            int? newCommunityId = (SelectedUserCommunity is null || SelectedUserCommunity.Id == 0)
                ? null
                : SelectedUserCommunity.Id;

            _admin.UpdateUserCommunity(SelectedUser.Id, newCommunityId);

            LoadUsers();

            MessageBox.Show(
                "Bendrija atnaujinta.",
                "OK",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
