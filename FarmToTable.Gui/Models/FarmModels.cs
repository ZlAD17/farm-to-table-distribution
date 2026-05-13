namespace FarmToTable.Gui.Models;

public class FarmResponse
{
    public int FarmId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateFarmRequest(string Name, string Location, string? ContactEmail, string? ContactPhone);
public record UpdateFarmRequest(string Name, string Location, string? ContactEmail, string? ContactPhone);