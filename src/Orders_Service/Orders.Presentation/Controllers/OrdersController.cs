using Microsoft.AspNetCore.Mvc;
using Orders.Presentation.Contracts.Orders;
using Orders.UseCases.Commands.CreateOrder;
using Orders.UseCases.Queries.GetOrders;
using Orders.UseCases.Queries.GetOrderById;
using Microsoft.AspNetCore.Http;
using Orders.UseCases.Utils;

namespace Orders.Presentation.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly CreateOrderHandler _createOrder;
    private readonly GetOrdersHandler _getOrders;
    private readonly GetOrderByIdHandler _getOrderById;

    public OrdersController(
        CreateOrderHandler createOrder,
        GetOrdersHandler getOrders,
        GetOrderByIdHandler getOrderById)
    {
        _createOrder = createOrder;
        _getOrders = getOrders;
        _getOrderById = getOrderById;
    }

    /// <summary>Создать заказ (асинхронная оплата)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var login = (request.Login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest("Login is required");
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Description is required");

        var publicId = await _createOrder.Handle(
            new CreateOrderCommand(login, request.Amount, request.Description),
            ct);

        return Created($"/orders/{publicId}", new CreateOrderResponse(publicId, "New"));
    }

    /// <summary>Получить список заказов пользователя по логину</summary>
    /// <remarks>Пример: GET /orders?login=vasya</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] string? login, CancellationToken ct)
    {
        var cleanLogin = (login ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleanLogin))
            return BadRequest("Login is required");

        var userId = DeterministicGuid.FromLogin(cleanLogin);

        var orders = await _getOrders.Handle(new GetOrdersQuery(userId), ct);

        var result = orders.Select(o =>
            new OrderListItemResponse(
                o.PublicId,
                o.Amount,
                o.Status.ToString()));

        return Ok(result);
    }

    /// <summary>Получить заказ по PublicId</summary>
    [HttpGet("{publicId}")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string publicId, CancellationToken ct)
    {
        var order = await _getOrderById.Handle(
            new GetOrderByIdQuery(publicId),
            ct);

        if (order == null)
            return NotFound();

        return Ok(new OrderDetailsResponse(
            order.PublicId,
            order.UserId,
            order.Amount,
            order.Description,
            order.Status.ToString()));
    }
}
