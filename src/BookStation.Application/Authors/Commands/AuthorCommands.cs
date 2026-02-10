using MediatR;

namespace BookStation.Application.Authors.Commands;

/// <summary>
/// Command to create a new author in the catalog. (Admin/Seller only)
/// </summary>
public record CreateAuthorCommand(
    string FullName,
    string? Bio = null,
    DateTime? DateOfBirth = null,
    string? Country = null
) : IRequest<CreateAuthorResponse>;

/// <summary>
/// Response for author creation.
/// </summary>
public record CreateAuthorResponse(
    long AuthorId,
    string FullName
);

/// <summary>
/// Command to update an existing author. (Admin only)
/// </summary>
public record UpdateAuthorCommand(
    long AuthorId,
    string FullName,
    string? Bio = null,
    DateTime? DateOfBirth = null,
    DateTime? DiedDate = null,
    string? Address = null,
    string? Country = null,
    string? PhotoUrl = null
) : IRequest<bool>;

/// <summary>
/// Command to delete an author. (Admin only)
/// </summary>
public record DeleteAuthorCommand(long AuthorId) : IRequest<bool>;
