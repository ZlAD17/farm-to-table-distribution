using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record CreateDriverRequest(
    [Required, MaxLength(150)] string FullName,
    [MaxLength(30)] string? Phone,
    [MaxLength(20)] string? LicensePlate
);

public record UpdateDriverRequest(
    [Required, MaxLength(150)] string FullName,
    [MaxLength(30)] string? Phone,
    [MaxLength(20)] string? LicensePlate
);
