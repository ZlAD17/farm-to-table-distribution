using System.ComponentModel.DataAnnotations;

namespace FarmToTable.Api.Models.Requests;

public record CreateCropRequest(
    [Required, MaxLength(100)] string Name,
    [Required, MaxLength(30)]  string Unit
);
