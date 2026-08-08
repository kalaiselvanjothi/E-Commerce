using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Order;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IInvoiceService _invoiceService;

    public OrdersController(IOrderService orderService, IInvoiceService invoiceService)
    {
        _orderService   = orderService;
        _invoiceService = invoiceService;
    }

    /// <summary>Generate and download professional PDF tax invoice</summary>
    [HttpGet("{id:guid}/invoice")]
    [ProducesResponseType(typeof(FileResult), 200)]
    public async Task<IActionResult> DownloadInvoice(Guid id)
    {
        var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(GetUserId(), id);
        return File(pdfBytes, "application/pdf", $"ShopVerse-Invoice-{id}.pdf");
    }

    /// <summary>Place a new order from the current cart</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), 201)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var order = await _orderService.CreateOrderAsync(GetUserId(), dto);
        return CreatedAtAction(nameof(GetOrder),
            new { id = order.Id },
            ApiResponse<OrderDetailDto>.Ok(order, "Order placed successfully"));
    }

    /// <summary>List all orders for the current user (paginated)</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderListDto>>), 200)]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetUserOrdersAsync(GetUserId(), page, pageSize);
        return Ok(ApiResponse<PagedResult<OrderListDto>>.Ok(result));
    }

    /// <summary>Get full order detail including timeline</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), 200)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderDetailAsync(GetUserId(), id);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order));
    }

    /// <summary>Cancel an order (only when Placed / Confirmed / Packed)</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), 200)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderDto dto)
    {
        var order = await _orderService.CancelOrderAsync(GetUserId(), id, dto.Reason);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order, "Order cancelled"));
    }

    /// <summary>Request a return for a delivered order</summary>
    [HttpPost("{id:guid}/return")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), 200)]
    public async Task<IActionResult> ReturnOrder(Guid id, [FromBody] ReturnOrderDto dto)
    {
        var order = await _orderService.RequestReturnAsync(GetUserId(), id, dto.Reason);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order, "Return request submitted"));
    }

    // ─── Admin ────────────────────────────────────────────────────────────────

    /// <summary>[Admin] List all orders with optional status filter</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await _orderService.GetAllOrdersAsync(page, pageSize, status);
        return Ok(ApiResponse<PagedResult<OrderListDto>>.Ok(result));
    }

    /// <summary>[Admin] Update order status (move through fulfilment pipeline)</summary>
    [HttpPatch("admin/{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, dto);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order, "Order status updated"));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
