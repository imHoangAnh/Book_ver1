using MediatR;

namespace BookStation.Application.Users.Commands;

// =====================================================
// SELLER MANAGEMENT COMMANDS (for SellersController)
// =====================================================

/// <summary>
/// Command to approve a seller application.
/// </summary>
public record ApproveSellerCommand(Guid UserId) : IRequest<SellerStatusResponse>;

/// <summary>
/// Command to reject a seller application.
/// </summary>
public record RejectSellerCommand(Guid UserId, string Reason) : IRequest<SellerStatusResponse>;

/// <summary>
/// Command to suspend a seller.
/// </summary>
public record SuspendSellerCommand(Guid UserId, string Reason) : IRequest<SellerStatusResponse>;

/// <summary>
/// Command to reactivate a seller.
/// </summary>
public record ReactivateSellerCommand(Guid UserId) : IRequest<SellerStatusResponse>;

/// <summary>
/// Response for seller status changes.
/// </summary>
public record SellerStatusResponse(
    Guid UserId,
    bool IsApproved,
    string Message
);
