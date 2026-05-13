namespace FarmToTable.Gui.Models;

public class OrderBatchLineRequest
{
    public int BatchId { get; set; }
    public decimal QuantityOrdered { get; set; }
}

public class CreateOrderRequest
{
    public int RestaurantId { get; set; }
    public int? DriverId { get; set; }
    public string? Notes { get; set; }
    public List<OrderBatchLineRequest> Batches { get; set; } = new();
}

public class UpdateOrderRequest
{
    public string Status { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? Notes { get; set; }
}

public class OrderResponse
{
    public int OrderId { get; set; }
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderBatchResponse> Batches { get; set; } = new();
}

public class OrderBatchResponse
{
    public int OrderBatchId { get; set; }
    public int BatchId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public decimal QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}