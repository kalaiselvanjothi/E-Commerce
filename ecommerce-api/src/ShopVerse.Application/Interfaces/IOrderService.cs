using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Order;

namespace ShopVerse.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDetailDto> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<PagedResult<OrderListDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize);
    Task<OrderDetailDto> GetOrderDetailAsync(Guid userId, Guid orderId);
    Task<OrderDetailDto> CancelOrderAsync(Guid userId, Guid orderId, string reason);
    Task<OrderDetailDto> RequestReturnAsync(Guid userId, Guid orderId, string reason);
    Task<OrderTrackResultDto?> TrackOrderAsync(string orderId, string? email = null);
    // Admin
    Task<PagedResult<OrderListDto>> GetAllOrdersAsync(int page, int pageSize, string? status = null);
    Task<OrderDetailDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto);
}
