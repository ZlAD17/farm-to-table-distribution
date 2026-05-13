namespace FarmToTable.Gui.Models;

public class CropResponse
{
    public int CropId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

public record CreateCropRequest(string Name, string Unit);