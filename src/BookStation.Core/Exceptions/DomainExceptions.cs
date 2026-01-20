namespace BookStation.Core.Exceptions;

/// <summary>
/// Base exception for domain-related errors.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found.")
    {
        EntityName = entityName;
        EntityId = id;
    }

    public string EntityName { get; }
    public object EntityId { get; }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string error)
        : base($"Validation failed for {propertyName}: {error}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { error } }
        };
    }

    public IDictionary<string, string[]> Errors { get; }
}
