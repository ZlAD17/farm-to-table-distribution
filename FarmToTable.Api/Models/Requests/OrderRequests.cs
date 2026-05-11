using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record OrderBatchLineRequest(
    [Range(1, int.MaxValue)]    int     BatchId,
    [Range(0.01, double.MaxValue)] decimal QuantityOrdered
);

public record CreateOrderRequest(
    [Range(1, int.MaxValue)] int RestaurantId,
    int? DriverId,
    [MaxLength(500)] string? Notes,
    [Required, MinLength(1)] IReadOnlyList<OrderBatchLineRequest> Batches
);

public record UpdateOrderRequest(
    [Required] string Status,   // Pending | Confirmed | Delivered | Cancelled
    int? DriverId,
    [MaxLength(500)] string? Notes
);
