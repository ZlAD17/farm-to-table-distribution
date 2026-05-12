namespace FarmToTable.Gui.Models;

public class RestaurantResponse
{
    public int RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateRestaurantRequest(string Name, string Address, string? ContactEmail, string? ContactPhone);
public record UpdateRestaurantRequest(string Name, string Address, string? ContactEmail, string? ContactPhone);