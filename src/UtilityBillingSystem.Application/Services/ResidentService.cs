using UtilityBillingSystem.Application.Abstractions;

namespace UtilityBillingSystem.Application.Services;

public sealed class ResidentService
{
    private readonly IPricingRepository _pricing;
    public ResidentService(IPricingRepository pricing) => _pricing = pricing;

    public List<ResidentServiceRow> GetMyServices(int userId) => _pricing.GetResidentServicesWithPrices(userId);
}
