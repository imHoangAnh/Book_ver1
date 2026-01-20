using BookStation.Core.SharedKernel;
using BookStation.Domain.Enums;

namespace BookStation.Domain.Entities.ShipmentAggregate;

/// <summary>
/// Shipment entity - Aggregate Root for shipment management.
/// </summary>
public class Shipment : AggregateRoot<long>
{
    /// <summary>
    /// Gets the order ID.
    /// </summary>
    public long OrderId { get; private set; }

    /// <summary>
    /// Gets the shipper user ID.
    /// </summary>
    public long? ShipperId { get; private set; }

    /// <summary>
    /// Gets the shipment status.
    /// </summary>
    public EShipmentStatus Status { get; private set; }

    /// <summary>
    /// Gets the tracking code.
    /// </summary>
    public string? TrackingCode { get; private set; }

    /// <summary>
    /// Gets the estimated delivery date.
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; private set; }

    /// <summary>
    /// Gets the actual delivery date.
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }

    /// <summary>
    /// Gets the pickup date.
    /// </summary>
    public DateTime? PickedUpAt { get; private set; }

    /// <summary>
    /// Gets any notes.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Shipment() { }

    /// <summary>
    /// Creates a new shipment.
    /// </summary>
    public static Shipment Create(long orderId, DateTime? estimatedDeliveryDate = null)
    {
        var shipment = new Shipment
        {
            OrderId = orderId,
            Status = EShipmentStatus.Pending,
            EstimatedDeliveryDate = estimatedDeliveryDate
        };

        shipment.AddDomainEvent(new ShipmentCreatedEvent(shipment));

        return shipment;
    }

    /// <summary>
    /// Assigns a shipper to this shipment.
    /// </summary>
    public void AssignShipper(long shipperId)
    {
        ShipperId = shipperId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShipperAssignedEvent(Id, shipperId));
    }

    /// <summary>
    /// Sets the tracking code.
    /// </summary>
    public void SetTrackingCode(string trackingCode)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
            throw new ArgumentException("Tracking code cannot be empty.", nameof(trackingCode));

        TrackingCode = trackingCode.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the shipment as picked up.
    /// </summary>
    public void PickUp()
    {
        if (Status != EShipmentStatus.Pending)
            throw new InvalidOperationException("Only pending shipments can be picked up.");

        Status = EShipmentStatus.PickedUp;
        PickedUpAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShipmentPickedUpEvent(Id));
    }

    /// <summary>
    /// Marks the shipment as in transit.
    /// </summary>
    public void StartTransit()
    {
        if (Status != EShipmentStatus.PickedUp)
            throw new InvalidOperationException("Only picked up shipments can start transit.");

        Status = EShipmentStatus.InTransit;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the shipment as out for delivery.
    /// </summary>
    public void OutForDelivery()
    {
        if (Status != EShipmentStatus.InTransit)
            throw new InvalidOperationException("Only in-transit shipments can be out for delivery.");

        Status = EShipmentStatus.OutForDelivery;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the shipment as delivered.
    /// </summary>
    public void Deliver()
    {
        if (Status is not (EShipmentStatus.InTransit or EShipmentStatus.OutForDelivery))
            throw new InvalidOperationException("Only in-transit or out-for-delivery shipments can be delivered.");

        Status = EShipmentStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShipmentDeliveredEvent(Id, OrderId));
    }

    /// <summary>
    /// Marks the shipment as failed.
    /// </summary>
    public void MarkFailed(string? notes = null)
    {
        Status = EShipmentStatus.Failed;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the shipment as returned.
    /// </summary>
    public void Return(string? notes = null)
    {
        Status = EShipmentStatus.Returned;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ShipmentReturnedEvent(Id, OrderId));
    }
}
