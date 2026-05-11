using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record CreateFarmRequest(
    [Required, MaxLength(150)] string Name,
    [Required, MaxLength(250)] string Location,
    [MaxLength(150)] string? ContactEmail,
    [MaxLength(30)]  string? ContactPhone
);

public record UpdateFarmRequest(
    [Required, MaxLength(150)] string Name,
    [Required, MaxLength(250)] string Location,
    [MaxLength(150)] string? ContactEmail,
    [MaxLength(30)]  string? ContactPhone
);
