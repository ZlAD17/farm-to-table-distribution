namespace FarmToTable.Gui.Models;

public class TopCropResult
{
    public int CropId { get; set; }
    public string CropName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}

public class FarmRevenueResult
{
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}