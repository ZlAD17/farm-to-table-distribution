namespace FarmToTable.Api.Models.Responses;

public record RestaurantResponse(
    int    RestaurantId,
    string Name,
    string Address,
    string? ContactEmail,
    string? ContactPhone,
    DateTime CreatedAt
);
