namespace FarmToTable.Api.Models.Responses;

public record CropResponse(
    int    CropId,
    string Name,
    string Unit
);
