namespace BookStation.Core.SharedKernel;

/// <summary>
/// Represents the Unit of Work pattern.
/// Maintains a list of objects affected by a business transaction
/// and coordinates the writing out of changes.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all pending changes and publishes domain events.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
/// <summary>
/// IDisposable: Provides a mechanism for releasing unmanaged resources.
/// La interface dung de giai phong tai nguyen khong duoc quan ly mot c
/// </summary>