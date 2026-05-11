namespace FarmToTable.Api.Models.Responses;

public record FarmResponse(
    int    FarmId,
    string Name,
    string Location,
    string? ContactEmail,
    string? ContactPhone,
    DateTime CreatedAt
);
