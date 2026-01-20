using System.Collections.ObjectModel;
using System.Globalization;
using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.Domain;
using UtilityBillingSystem.UI.Wpf.Mvvm;

namespace UtilityBillingSystem.UI.Wpf.ViewModels;

public sealed class ManagerViewModel : ViewModelBase
{
    private readonly AdminService _admin;
    private readonly ManagerService _manager;

    public ObservableCollection<Community> Communities { get; } = new();
    public ObservableCollection<Service> Services { get; } = new();
    public ObservableCollection<AssignedServiceRow> AssignedServices { get; } = new();

    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }

    // UI navigation (tabs)
    private int _selectedTabIndex;
    public int SelectedTabIndex { get => _selectedTabIndex; set => Set(ref _selectedTabIndex, value); }

    private Community? _selectedCommunity;
    public Community? SelectedCommunity
    {
        get => _selectedCommunity;
        set
        {
            if (Set(ref _selectedCommunity, value))
                LoadAssigned();
        }
    }

    private Service? _selectedService;
    public Service? SelectedService { get => _selectedService; set => Set(ref _selectedService, value); }

    private string _currentPriceText = "Pasirink bendriją ir paslaugą.";
    public string CurrentPriceText { get => _currentPriceText; set => Set(ref _currentPriceText, value); }

    private string _newPrice = "";
    public string NewPrice { get => _newPrice; set => Set(ref _newPrice, value); }

    private DateTime? _effectiveFrom = DateTime.Today;
    public DateTime? EffectiveFrom { get => _effectiveFrom; set => Set(ref _effectiveFrom, value); }

    public RelayCommand AssignCommand { get; }
    public RelayCommand LoadCurrentPriceCommand { get; }
    public RelayCommand SetPriceCommand { get; }

    public ManagerViewModel(AdminService admin, ManagerService manager)
    {
        _admin = admin;
        _manager = manager;

        AssignCommand = new RelayCommand(Assign);
        LoadCurrentPriceCommand = new RelayCommand(LoadCurrentPrice);
        SetPriceCommand = new RelayCommand(SetPrice);

        LoadLists();
    }

    private void LoadLists()
    {
        ErrorMessage = "";

        Communities.Clear();
        foreach (var c in _admin.GetAllCommunities())
            Communities.Add(c);

        Services.Clear();
        foreach (var s in _admin.GetAllServices(onlyActive: true))
            Services.Add(s);

        SelectedCommunity = Communities.FirstOrDefault();
        SelectedService = Services.FirstOrDefault();
        LoadAssigned();
    }

    private void LoadAssigned()
    {
        AssignedServices.Clear();
        if (SelectedCommunity is null) return;

        foreach (var row in _manager.GetAssignedServices(SelectedCommunity.Id))
            AssignedServices.Add(row);
    }

    private void Assign()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedCommunity is null) throw new InvalidOperationException("Pasirink bendriją.");
            if (SelectedService is null) throw new InvalidOperationException("Pasirink paslaugą.");

            _manager.AssignService(SelectedCommunity.Id, SelectedService.Id);
            LoadAssigned();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void LoadCurrentPrice()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedCommunity is null) throw new InvalidOperationException("Pasirink bendriją.");
            if (SelectedService is null) throw new InvalidOperationException("Pasirink paslaugą.");

            var cp = _manager.GetCurrentPrice(SelectedCommunity.Id, SelectedService.Id);
            CurrentPriceText = cp is null
                ? "Kaina nenustatyta."
                : $"Dabartinė kaina: {cp.Price} {cp.Currency} (nuo {cp.EffectiveFrom:yyyy-MM-dd})";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void SetPrice()
    {
        try
        {
            ErrorMessage = "";
            if (SelectedCommunity is null) throw new InvalidOperationException("Pasirink bendriją.");
            if (SelectedService is null) throw new InvalidOperationException("Pasirink paslaugą.");
            if (EffectiveFrom is null) throw new InvalidOperationException("Parink galiojimo datą.");

            if (!decimal.TryParse(NewPrice, NumberStyles.Number, CultureInfo.CurrentCulture, out var price))
                throw new InvalidOperationException("Neteisingas kainos formatas.");

            _manager.SetNewPrice(SelectedCommunity.Id, SelectedService.Id, price, DateOnly.FromDateTime(EffectiveFrom.Value));
            LoadCurrentPrice();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
