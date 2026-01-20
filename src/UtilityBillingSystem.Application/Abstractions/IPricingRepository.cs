namespace UtilityBillingSystem.Application.Abstractions;

public interface IPricingRepository
{
    CurrentPriceRow? GetCurrentPrice(int communityServiceId);
    void SetNewPrice(int communityServiceId, decimal price, string currency, DateOnly effectiveFrom);
    List<ResidentServiceRow> GetResidentServicesWithPrices(int userId);
}

public sealed record CurrentPriceRow(decimal Price, string Currency, DateOnly EffectiveFrom);
public sealed record ResidentServiceRow(string ServiceName, decimal Price, string Currency, DateOnly EffectiveFrom);
