using System;
using System.Collections.ObjectModel;
using System.Windows;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.Domain.Users;
using UtilityBillingSystem.UI.Wpf.Mvvm;
using UtilityBillingSystem.UI.Wpf.Views;

namespace UtilityBillingSystem.UI.Wpf.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly AdminService _admin;
    private readonly ManagerService _manager;
    private readonly ResidentService _resident;

    public User CurrentUser { get; }

    public string HeaderText => $"{CurrentUser.FirstName} {CurrentUser.LastName} ({CurrentUser.Username})";
    public string RoleText => CurrentUser.Role.ToString();


    public ObservableCollection<string> MenuItems { get; } = new();

    private string? _selectedMenuItem;
    public string? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            if (Set(ref _selectedMenuItem, value))
                UpdateContent();
        }
    }

    private object? _currentContent;
    public object? CurrentContent { get => _currentContent; set => Set(ref _currentContent, value); }

    // Cache views/viewmodels so selecting menu doesn't reload all data every time
    private AdminViewModel? _adminVm;
    private AdminView? _adminView;
    private ManagerViewModel? _managerVm;
    private ManagerView? _managerView;
    private ResidentViewModel? _residentVm;
    private ResidentView? _residentView;

    public event EventHandler? RequestLogout;

    public RelayCommand LogoutCommand { get; }
    public RelayCommand ExitCommand { get; }

    public MainViewModel(User user, AdminService admin, ManagerService manager, ResidentService resident)
    {
        CurrentUser = user;
        _admin = admin;
        _manager = manager;
        _resident = resident;

        foreach (var item in user.GetMenuItems())
            MenuItems.Add(item);

        SelectedMenuItem = MenuItems.FirstOrDefault();

        LogoutCommand = new RelayCommand(() => RequestLogout?.Invoke(this, EventArgs.Empty));
        ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
    }

    private void UpdateContent()
    {
        if (SelectedMenuItem is null) return;

        switch (CurrentUser.Role)
        {
            case UserRole.Admin:
                {
                    // Admin can also use manager screens
                    if (SelectedMenuItem is "Priskyrimas bendrijoms" or "Kainų valdymas")
                    {
                        _managerVm ??= new ManagerViewModel(_admin, _manager);
                        _managerView ??= new ManagerView { DataContext = _managerVm };

                        _managerVm.SelectedTabIndex = SelectedMenuItem switch
                        {
                            "Priskyrimas bendrijoms" => 0,
                            "Kainų valdymas" => 1,
                            _ => 0
                        };

                        CurrentContent = _managerView;
                        break;
                    }

                    // Admin can also see the resident screen ("My services")
                    if (SelectedMenuItem is "Mano paslaugos")
                    {
                        _residentVm ??= new ResidentViewModel(_resident, CurrentUser.Id);
                        _residentView ??= new ResidentView { DataContext = _residentVm };

                        CurrentContent = _residentView;
                        break;
                    }

                    // Default admin screens
                    _adminVm ??= new AdminViewModel(_admin);
                    _adminView ??= new AdminView { DataContext = _adminVm };

                    _adminVm.SelectedTabIndex = SelectedMenuItem switch
                    {
                        "Bendrijos" => 0,
                        "Paslaugos" => 1,
                        "Vartotojai" => 2,
                        _ => 0
                    };

                    CurrentContent = _adminView;
                    break;
                }



            case UserRole.Manager:
                {
                    // Manager can also see resident screen ("My services")
                    if (SelectedMenuItem is "Mano paslaugos")
                    {
                        _residentVm ??= new ResidentViewModel(_resident, CurrentUser.Id);
                        _residentView ??= new ResidentView { DataContext = _residentVm };

                        CurrentContent = _residentView;
                        break;
                    }

                    // Default manager screens
                    _managerVm ??= new ManagerViewModel(_admin, _manager);
                    _managerView ??= new ManagerView { DataContext = _managerVm };

                    _managerVm.SelectedTabIndex = SelectedMenuItem switch
                    {
                        "Priskyrimas bendrijoms" => 0,
                        "Kainų valdymas" => 1,
                        _ => 0
                    };

                    CurrentContent = _managerView;
                    break;
                }

            case UserRole.Resident:
                _residentVm ??= new ResidentViewModel(_resident, CurrentUser.Id);
                _residentView ??= new ResidentView { DataContext = _residentVm };
                CurrentContent = _residentView;
                break;
        }
    }
}
