using BookStation.Core.SharedKernel;

namespace BookStation.Domain.Entities.ShipmentAggregate;

/// <summary>
/// Base event for shipment-related domain events.
/// </summary>
public abstract class ShipmentBaseEvent : DomainEvent
{
    public long ShipmentId { get; }

    protected ShipmentBaseEvent(long shipmentId)
    {
        ShipmentId = shipmentId;
    }
}

/// <summary>
/// Event raised when a shipment is created.
/// </summary>
public sealed class ShipmentCreatedEvent : ShipmentBaseEvent
{
    public long OrderId { get; }

    public ShipmentCreatedEvent(Shipment shipment) : base(shipment.Id)
    {
        OrderId = shipment.OrderId;
    }
}


/// <summary>
/// Event raised when a shipment is picked up.
/// </summary>
public sealed class ShipmentPickedUpEvent : ShipmentBaseEvent
{
    public ShipmentPickedUpEvent(long shipmentId) : base(shipmentId)
    {
    }
}

/// <summary>
/// Event raised when a shipment is delivered.
/// </summary>
public sealed class ShipmentDeliveredEvent : ShipmentBaseEvent
{
    public long OrderId { get; }

    public ShipmentDeliveredEvent(long shipmentId, long orderId) : base(shipmentId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// Event raised when a shipment is returned.
/// </summary>
public sealed class ShipmentReturnedEvent : ShipmentBaseEvent
{
    public long OrderId { get; }

    public ShipmentReturnedEvent(long shipmentId, long orderId) : base(shipmentId)
    {
        OrderId = orderId;
    }
}
