namespace BookStation.Domain.Enums;

/// <summary>
/// Shipment status.
/// </summary>
public enum EShipmentStatus
{
    /// <summary>Shipment is pending.</summary>
    Pending = 0,

    /// <summary>Package has been picked up.</summary>
    PickedUp = 1,

    /// <summary>Package is in transit.</summary>
    InTransit = 2,

    /// <summary>Package is out for delivery.</summary>
    OutForDelivery = 3,

    /// <summary>Package has been delivered.</summary>
    Delivered = 4,

    /// <summary>Package was returned.</summary>
    Returned = 5,

    /// <summary>Delivery failed.</summary>
    Failed = 6
}
