namespace Shared.Domain.Authorization.Requirements;

public class ResourceOwnershipRequirement : IAuthorizationRequirement
{
    public Guid ResourceOwnerId { get; set; }
    public Guid? ResourceId { get; set; }
    public string? ResourceType { get; set; }

    public ResourceOwnershipRequirement(Guid resourceOwnerId)
    {
        ResourceOwnerId = resourceOwnerId;
    }

    public ResourceOwnershipRequirement(Guid resourceOwnerId, Guid resourceId, string resourceType)
    {
        ResourceOwnerId = resourceOwnerId;
        ResourceId = resourceId;
        ResourceType = resourceType;
    }
}
