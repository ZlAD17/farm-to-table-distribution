namespace FarmToTable.Api.Models.Responses;

public record DeliveryTripResponse(
    int      TripId,
    int      DriverId,
    string   DriverName,
    int      OrderId,
    DateTime ScheduledAt,
    DateTime? CompletedAt,
    string?  Notes
);
