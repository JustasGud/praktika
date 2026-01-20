using System.Windows;
using UtilityBillingSystem.Application.Auth;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.UI.Wpf.ViewModels;

namespace UtilityBillingSystem.UI.Wpf.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(AuthService auth, AdminService admin, ManagerService manager, ResidentService resident)
    {
        InitializeComponent();
        _vm = new LoginViewModel(auth, admin, manager, resident);
        DataContext = _vm;
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        _vm.Password = PwdBox.Password;
        _vm.Login();

        if (_vm.IsLoggedIn && _vm.NextWindow is not null)
        {
            _vm.NextWindow.Show();
            Close();
        }
    }
}
