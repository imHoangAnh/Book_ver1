using BookStation.Application.Orders.Commands;
using BookStation.Query.Orders;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookStation.PublicApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get order details by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get orders for current user.
    /// </summary>
    [HttpGet("my-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders()
    {
        // TODO: Get user ID from authentication
        // TODO: Implement GetUserOrdersQuery
        return Ok(new { message = "My orders endpoint - to be implemented" });
    }
}
