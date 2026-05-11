namespace FarmToTable.Api.Models.Responses;

public record DriverResponse(
    int    DriverId,
    string FullName,
    string? Phone,
    string? LicensePlate,
    DateTime CreatedAt
);
