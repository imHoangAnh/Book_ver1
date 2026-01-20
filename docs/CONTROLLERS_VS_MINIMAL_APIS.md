# Controllers vs Minimal APIs - So sánh

## 📝 Controllers (Hiện tại đang dùng)

```csharp
// File: Controllers/BooksController.cs
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search books with filters and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BookListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchBooksQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get book details by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetBookByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Create a new book.
    /// </summary>
    [HttpPost]
    [Authorize]
    [RequirePermission("books.create")]
    [ProducesResponseType(typeof(CreateBookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.BookId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

**Ưu điểm Controllers:**
- ✅ Tổ chức tốt, dễ hiểu (OOP style)
- ✅ Có base class với nhiều helper methods
- ✅ Dễ dàng group related endpoints
- ✅ Swagger tự động generate tốt
- ✅ Dependency injection qua constructor
- ✅ Dễ test với mocking

**Nhược điểm Controllers:**
- ❌ Boilerplate code nhiều
- ❌ Phải tạo class cho mỗi controller
- ❌ Performance chậm hơn một chút (do reflection)

---

## 🚀 Minimal APIs (Alternative)

```csharp
// File: Program.cs hoặc Endpoints/BookEndpoints.cs

// Cách 1: Inline trong Program.cs
var app = builder.Build();

// Books Endpoints
var booksGroup = app.MapGroup("/api/books")
    .WithTags("Books")
    .WithOpenApi();

// GET /api/books - Search books
booksGroup.MapGet("/", async (
    [AsParameters] SearchBooksQuery query,
    IMediator mediator) =>
{
    var result = await mediator.Send(query);
    return Results.Ok(result);
})
.WithName("SearchBooks")
.WithSummary("Search books with filters and pagination")
.Produces<PaginatedList<BookListDto>>(StatusCodes.Status200OK);

// GET /api/books/{id} - Get book by ID
booksGroup.MapGet("/{id}", async (
    long id,
    IMediator mediator) =>
{
    var query = new GetBookByIdQuery(id);
    var result = await mediator.Send(query);
    
    return result == null 
        ? Results.NotFound() 
        : Results.Ok(result);
})
.WithName("GetBookById")
.WithSummary("Get book details by ID")
.Produces<BookDetailDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// POST /api/books - Create book
booksGroup.MapPost("/", async (
    CreateBookCommand command,
    IMediator mediator) =>
{
    try
    {
        var result = await mediator.Send(command);
        return Results.CreatedAtRoute("GetBookById", new { id = result.BookId }, result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateBook")
.WithSummary("Create a new book")
.RequireAuthorization("books.create") // Permission-based
.Produces<CreateBookResponse>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

// ===================================================
// Cách 2: Extension method (Clean hơn)
// ===================================================

// File: Endpoints/BookEndpoints.cs
public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books")
            .WithTags("Books")
            .WithOpenApi();

        group.MapGet("/", SearchBooks)
            .WithName("SearchBooks")
            .Produces<PaginatedList<BookListDto>>();

        group.MapGet("/{id}", GetBookById)
            .WithName("GetBookById")
            .Produces<BookDetailDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBook)
            .WithName("CreateBook")
            .RequireAuthorization("books.create")
            .Produces<CreateBookResponse>(StatusCodes.Status201Created);

        return app;
    }

    private static async Task<IResult> SearchBooks(
        [AsParameters] SearchBooksQuery query,
        IMediator mediator)
    {
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetBookById(
        long id,
        IMediator mediator)
    {
        var query = new GetBookByIdQuery(id);
        var result = await mediator.Send(query);
        return result == null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> CreateBook(
        CreateBookCommand command,
        IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(command);
            return Results.CreatedAtRoute("GetBookById", new { id = result.BookId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

// Trong Program.cs
app.MapBookEndpoints();
app.MapAuthEndpoints();
app.MapOrderEndpoints();
```

**Ưu điểm Minimal APIs:**
- ✅ Ít boilerplate code hơn
- ✅ Performance tốt hơn (ít reflection)
- ✅ Functional programming style
- ✅ Dễ dàng tạo micro-endpoints
- ✅ Startup nhanh hơn
- ✅ Modern, trendy (C# 10+)

**Nhược điểm Minimal APIs:**
- ❌ Khó tổ chức khi project lớn
- ❌ Không có base class helpers
- ❌ Swagger metadata phải config thủ công nhiều hơn
- ❌ Dependency injection qua parameters (có thể rối)
- ❌ Khó test hơn một chút

---

## 📊 So sánh trực tiếp

| Tiêu chí | Controllers | Minimal APIs |
|----------|-------------|--------------|
| **Boilerplate** | Nhiều | Ít |
| **Performance** | Chậm hơn ~5% | Nhanh hơn |
| **Tổ chức code** | Tốt (OOP) | Tốt nếu dùng extension methods |
| **Swagger** | Tự động tốt | Cần config thủ công |
| **Testing** | Dễ | Hơi khó |
| **Learning curve** | Dễ | Trung bình |
| **Phù hợp** | Large apps | Microservices, small apps |

---

## 🎯 Kết luận

**Dùng Controllers khi:**
- ✅ Dự án lớn, nhiều endpoints
- ✅ Team quen với OOP
- ✅ Cần tổ chức code rõ ràng
- ✅ Muốn Swagger tự động tốt

**Dùng Minimal APIs khi:**
- ✅ Microservices
- ✅ Performance quan trọng
- ✅ Dự án nhỏ, prototype
- ✅ Team thích functional programming

**BookStation hiện tại:** Dùng **Controllers** vì:
- Dự án lớn, nhiều bounded contexts
- Dễ maintain và scale
- Team dễ hiểu và collaborate

Nhưng bạn hoàn toàn có thể convert sang Minimal APIs nếu muốn! 🚀
