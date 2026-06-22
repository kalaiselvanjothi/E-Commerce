namespace ShopVerse.Domain.Enums;

public enum OrderStatus
{
    Placed = 1,
    Confirmed = 2,
    Packed = 3,
    Shipped = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7,
    ReturnRequested = 8,
    Returned = 9
}
