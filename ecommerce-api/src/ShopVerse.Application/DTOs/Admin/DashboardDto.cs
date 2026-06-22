namespace ShopVerse.Application.DTOs.Admin;

// Flat structure to match the frontend DashboardStats model
public class DashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public double RevenueChange { get; set; }
    public int TotalOrders { get; set; }
    public double OrdersChange { get; set; }
    public int TotalCustomers { get; set; }
    public double CustomersChange { get; set; }
    public int TotalProducts { get; set; }
    public int PendingOrders { get; set; }
    public List<ChartPointDto> RevenueChart { get; set; } = new();
    public List<ChartPointDto> OrdersChart { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

public class ChartPointDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class TopProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalSold { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
