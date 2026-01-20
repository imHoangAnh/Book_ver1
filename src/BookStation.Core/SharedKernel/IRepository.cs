namespace BookStation.Core.SharedKernel;

/// <summary>
/// Base repository interface for write operations.
/// </summary>
/// <typeparam name="TEntity">The type of entity.</typeparam>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Adds an entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
}



/* doi ten thanh irepository 
IRepository<TEntity, TId>
public interface IWriteOnlyRepository
{
    Task<TEntity?> GetByIdAsync(...)
}

IReadRepository<TEntity, TId>
IWriteRepository<TEntity, TId>
BookStation.Core.SharedKernel
├── Entity.cs
├── AggregateRoot.cs
├── ValueObject.cs
├── Result.cs
├── IDomainEvent.cs
├── DomainEvent.cs
├── IRepository.cs
├── IUnitOfWork.cs

*/
