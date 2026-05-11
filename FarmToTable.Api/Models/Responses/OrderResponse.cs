namespace FarmToTable.Api.Models.Responses;

public record OrderBatchResponse(
    int     OrderBatchId,
    int     BatchId,
    string  CropName,
    string  FarmName,
    decimal QuantityOrdered,
    decimal UnitPrice,
    decimal LineTotal
);

public record OrderResponse(
    int      OrderId,
    int      RestaurantId,
    string   RestaurantName,
    int?     DriverId,
    string?  DriverName,
    string   Status,
    DateTime OrderedAt,
    DateTime? DeliveredAt,
    string?  Notes,
    decimal  TotalAmount,
    IReadOnlyList<OrderBatchResponse> Batches
);
