using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record CreateHarvestBatchRequest(
    [Range(1, int.MaxValue)] int FarmId,
    [Range(1, int.MaxValue)] int CropId,
    [Range(0.01, double.MaxValue)] decimal QuantityAvailable,
    DateOnly HarvestDate,
    [Range(0.01, double.MaxValue)] decimal PricePerUnit
);

public record UpdateHarvestBatchRequest(
    [Range(0.01, double.MaxValue)] decimal QuantityAvailable,
    [Range(0.01, double.MaxValue)] decimal PricePerUnit,
    [Required] string Status   // Available | Sold | Expired
);

public record HarvestBatchFilterRequest(
    int?      FarmId,
    int?      CropId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    string?   Status
);
