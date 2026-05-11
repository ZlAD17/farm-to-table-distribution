using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record CreateRestaurantRequest(
    [Required, MaxLength(150)] string Name,
    [Required, MaxLength(300)] string Address,
    [MaxLength(150)] string? ContactEmail,
    [MaxLength(30)]  string? ContactPhone
);

public record UpdateRestaurantRequest(
    [Required, MaxLength(150)] string Name,
    [Required, MaxLength(300)] string Address,
    [MaxLength(150)] string? ContactEmail,
    [MaxLength(30)]  string? ContactPhone
);
