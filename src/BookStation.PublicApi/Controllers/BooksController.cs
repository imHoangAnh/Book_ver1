using BookStation.Application.Books.Commands;
using BookStation.Query.Books;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookStation.PublicApi.Controllers;

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
