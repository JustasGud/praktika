using UtilityBillingSystem.Application.Abstractions;

namespace UtilityBillingSystem.Application.Services;

public sealed class ManagerService
{
    private readonly ICommunityServiceRepository _cs;
    private readonly IPricingRepository _pricing;

    public ManagerService(ICommunityServiceRepository cs, IPricingRepository pricing)
    {
        _cs = cs;
        _pricing = pricing;
    }

    public int AssignService(int communityId, int serviceId) => _cs.AssignService(communityId, serviceId);

    public List<AssignedServiceRow> GetAssignedServices(int communityId) => _cs.GetAssignedServices(communityId);

    public CurrentPriceRow? GetCurrentPrice(int communityId, int serviceId)
    {
        var csId = _cs.GetCommunityServiceId(communityId, serviceId);
        return csId is null ? null : _pricing.GetCurrentPrice(csId.Value);
    }

    public void SetNewPrice(int communityId, int serviceId, decimal price, DateOnly effectiveFrom)
    {
        var csId = _cs.GetCommunityServiceId(communityId, serviceId)
                  ?? throw new InvalidOperationException("Paslauga nepriskirta šiai bendrijai.");

        _pricing.SetNewPrice(csId, price, "EUR", effectiveFrom);
    }
}
