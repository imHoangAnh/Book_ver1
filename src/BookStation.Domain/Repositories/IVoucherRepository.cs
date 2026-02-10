using BookStation.Core.SharedKernel;
using BookStation.Domain.Entities.VoucherAggregate;

namespace BookStation.Domain.Repositories;

/// <summary>
/// Repository interface for Voucher aggregate.
/// </summary>
public interface IVoucherRepository : IRepository<Voucher, long>
{
    /// <summary>
    /// Gets a voucher by code.
    /// </summary>
    Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a voucher code is unique.
    /// </summary>
    Task<bool> IsCodeUniqueAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active vouchers for an organization.
    /// </summary>
    Task<IEnumerable<Voucher>> GetActiveByOrganizationAsync(int organizationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available vouchers that can be applied to an order.
    /// </summary>
    Task<IEnumerable<Voucher>> GetAvailableVouchersAsync(ValueObjects.Money orderAmount, long userId, CancellationToken cancellationToken = default);
}
