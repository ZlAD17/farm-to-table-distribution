namespace FarmToTable.Api.Models.Responses;

public record HarvestBatchResponse(
    int      BatchId,
    int      FarmId,
    string   FarmName,
    int      CropId,
    string   CropName,
    string   CropUnit,
    decimal  QuantityAvailable,
    decimal  QuantityRemaining,
    DateOnly HarvestDate,
    decimal  PricePerUnit,
    string   Status,
    DateTime CreatedAt
);
