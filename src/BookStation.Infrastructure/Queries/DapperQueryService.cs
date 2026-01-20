using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BookStation.Infrastructure.Queries;

/// <summary>
/// Dapper-based query service for complex raw SQL queries.
/// Use this for performance-critical read operations.
/// </summary>
public class DapperQueryService
{
    private readonly string _connectionString;

    public DapperQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    /// <summary>
    /// Execute a query and return a list of results.
    /// </summary>
    public async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Execute a query and return a single result.
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
    }

    /// <summary>
    /// Execute a command (INSERT, UPDATE, DELETE).
    /// </summary>
    public async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteAsync(sql, parameters);
    }

    /// <summary>
    /// Execute a query with multiple result sets.
    /// </summary>
    public async Task<(IEnumerable<T1>, IEnumerable<T2>)> QueryMultipleAsync<T1, T2>(
        string sql,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(sql, parameters);
        
        var result1 = await multi.ReadAsync<T1>();
        var result2 = await multi.ReadAsync<T2>();
        
        return (result1, result2);
    }
}

/// <summary>
/// Example: Complex book search query using Dapper for better performance.
/// </summary>
public class BookDapperQueries
{
    private readonly DapperQueryService _dapper;

    public BookDapperQueries(DapperQueryService dapper)
    {
        _dapper = dapper;
    }

    public async Task<IEnumerable<BookSearchResult>> SearchBooksOptimized(
        string? searchTerm,
        int pageNumber,
        int pageSize)
    {
        var sql = @"
            SELECT 
                b.Id,
                b.Title,
                b.ISBN,
                b.CoverImageUrl,
                MIN(v.Price) as MinPrice,
                MAX(v.Price) as MaxPrice,
                STRING_AGG(a.FullName, ', ') as Authors,
                COUNT(DISTINCT r.Id) as ReviewCount,
                AVG(CAST(r.Rating as FLOAT)) as AverageRating
            FROM Books b
            LEFT JOIN BookVariants v ON b.Id = v.BookId
            LEFT JOIN BookAuthors ba ON b.Id = ba.BookId
            LEFT JOIN Authors a ON ba.AuthorId = a.Id
            LEFT JOIN Reviews r ON b.Id = r.BookId
            WHERE (@SearchTerm IS NULL OR 
                   b.Title LIKE '%' + @SearchTerm + '%' OR
                   b.ISBN LIKE '%' + @SearchTerm + '%')
            GROUP BY b.Id, b.Title, b.ISBN, b.CoverImageUrl
            ORDER BY b.Title
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            SearchTerm = searchTerm,
            Offset = (pageNumber - 1) * pageSize,
            PageSize = pageSize
        };

        return await _dapper.QueryAsync<BookSearchResult>(sql, parameters);
    }
}

public record BookSearchResult(
    long Id,
    string Title,
    string? ISBN,
    string? CoverImageUrl,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Authors,
    int ReviewCount,
    double? AverageRating
);
