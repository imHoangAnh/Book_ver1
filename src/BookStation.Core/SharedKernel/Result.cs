namespace BookStation.Core.SharedKernel;

/// <summary>
/// Represents the result of an operation.
/// Can be successful or contain errors.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess && error != null)
            throw new ArgumentException("A success result cannot have an error.", nameof(error));

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("A failure result must have an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success<T>(T value) => new(value, true, null);

    /// <summary>
    /// Creates a failure result with the specified error message.
    /// </summary>
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

/// <summary>
/// Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value on a failed result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value on a failed result.");

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Result.Success(value);
}
