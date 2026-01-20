using System.Windows;
using UtilityBillingSystem.UI.Wpf.ViewModels;

namespace UtilityBillingSystem.UI.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        // Atsijungimas: gr?žtame ? prisijungimo lang?, uždarome pagrindin? lang?.
        vm.RequestLogout += (_, __) =>
        {
            ((App)System.Windows.Application.Current).ShowLoginWindow();
            Close();
        };
    }
}
