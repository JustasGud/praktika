namespace UtilityBillingSystem.Application.Abstractions;

public interface ICommunityServiceRepository
{
    int AssignService(int communityId, int serviceId);
    int? GetCommunityServiceId(int communityId, int serviceId);
    List<AssignedServiceRow> GetAssignedServices(int communityId);
}

public sealed record AssignedServiceRow(int CommunityServiceId, int ServiceId, string ServiceName);
