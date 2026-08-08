namespace ShopVerse.Application.DTOs.Order;

public class OrderListDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public string? PrimaryImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderDetailDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryOption { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; }
    public string? CouponCode { get; set; }
    public decimal Total { get; set; }
    public List<StatusHistoryDto> StatusHistory { get; set; } = new();
    public string? EstimatedDelivery { get; set; }
    public string? RazorpayOrderId { get; set; }
    public string? CancelReason { get; set; }
    public string? ReturnReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class StatusHistoryDto
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ShippingAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class OrderTrackResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = "Completed";
    public int CurrentStep { get; set; }
    public string EstimatedDeliveryDate { get; set; } = string.Empty;
    public string CourierName { get; set; } = "BlueDart Express";
    public string TrackingNumber { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<TrackItemDto> Items { get; set; } = new();
    public List<TimelineStepDto> Timeline { get; set; } = new();
}

public class TrackItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; } = string.Empty;
}

public class TimelineStepDto
{
    public string Step { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public bool Current { get; set; }
}
