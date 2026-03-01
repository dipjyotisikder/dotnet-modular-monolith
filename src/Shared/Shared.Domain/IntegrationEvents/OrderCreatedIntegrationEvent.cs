namespace Shared.Domain.IntegrationEvents;

public record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    decimal Total,
    DateTime CreatedAt,
    List<OrderItemDto> Items);

public record OrderItemDto(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
