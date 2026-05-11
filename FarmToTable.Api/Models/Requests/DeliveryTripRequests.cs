using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record CreateDeliveryTripRequest(
    [Range(1, int.MaxValue)] int DriverId,
    [Range(1, int.MaxValue)] int OrderId,
    [Required] DateTime ScheduledAt,
    [MaxLength(500)] string? Notes
);

public record CompleteDeliveryTripRequest(
    DateTime CompletedAt,
    [MaxLength(500)] string? Notes
);
