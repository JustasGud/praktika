using System.Windows;
using UtilityBillingSystem.Application.Auth;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.UI.Wpf.Mvvm;
using UtilityBillingSystem.UI.Wpf.Views;

namespace UtilityBillingSystem.UI.Wpf.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly AuthService _auth;
    private readonly AdminService _admin;
    private readonly ManagerService _manager;
    private readonly ResidentService _resident;

    private string _username = "";
    public string Username { get => _username; set => Set(ref _username, value); }

    public string Password { get; set; } = "";

    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }

    public bool IsLoggedIn { get; private set; }
    public Window? NextWindow { get; private set; }

    public LoginViewModel(AuthService auth, AdminService admin, ManagerService manager, ResidentService resident)
    {
        _auth = auth;
        _admin = admin;
        _manager = manager;
        _resident = resident;
    }

    public void Login()
    {
        ErrorMessage = "";
        try
        {
            var user = _auth.Login(Username.Trim(), Password);

            var mainVm = new MainViewModel(user, _admin, _manager, _resident);
            NextWindow = new MainWindow(mainVm);

            IsLoggedIn = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsLoggedIn = false;
            NextWindow = null;
        }
    }
}
