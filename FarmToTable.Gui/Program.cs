using FarmToTable.Gui.Services;

namespace FarmToTable.Gui;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        var apiService = new ApiService("http://localhost:5000");
        
        var farmService = new FarmGuiService(apiService);
        var cropService = new CropGuiService(apiService);
        var restaurantService = new RestaurantGuiService(apiService);
        var orderService = new OrderGuiService(apiService);
        var reportService = new ReportGuiService(apiService);
        
        Application.Run(new MainForm(farmService, cropService, restaurantService, orderService, reportService));
    }
}