using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Application.Services;
using UtilityBillingSystem.UI.Wpf.Mvvm;

namespace UtilityBillingSystem.UI.Wpf.ViewModels;

public sealed class ResidentViewModel : ViewModelBase
{
    private readonly ResidentService _resident;
    private readonly int _userId;

    public ObservableCollection<ResidentServiceRowVm> Services { get; } = new();

    public ICollectionView FilteredServices { get; }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (Set(ref _searchText, value))
                FilteredServices.Refresh();
        }
    }

    private string _errorMessage = "";
    public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }

    public RelayCommand RefreshCommand { get; }

    public ResidentViewModel(ResidentService resident, int userId)
    {
        _resident = resident;
        _userId = userId;

        FilteredServices = CollectionViewSource.GetDefaultView(Services);
        FilteredServices.Filter = Filter;

        RefreshCommand = new RelayCommand(Refresh);

        Refresh();
    }

    private bool Filter(object obj)
    {
        if (obj is not ResidentServiceRowVm row) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return row.ServiceName.Contains(SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase);
    }

    private void Refresh()
    {
        try
        {
            ErrorMessage = "";

            Services.Clear();
            foreach (var r in _resident.GetMyServices(_userId))
            {
                Services.Add(new ResidentServiceRowVm
                {
                    ServiceName = r.ServiceName,
                    Price = r.Price,
                    Currency = r.Currency,
                    EffectiveFrom = r.EffectiveFrom.ToString("yyyy-MM-dd")
                });
            }

            FilteredServices.Refresh();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}

public sealed class ResidentServiceRowVm
{
    public string ServiceName { get; set; } = "";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "";
    public string EffectiveFrom { get; set; } = "";
}
