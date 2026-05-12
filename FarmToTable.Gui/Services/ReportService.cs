using FarmToTable.Gui.Models;

namespace FarmToTable.Gui.Services;

public class ReportGuiService
{
    private readonly ApiService _api;

    public ReportGuiService(ApiService api) => _api = api;

    public async Task<List<TopCropResult>> GetTopCropAsync(DateOnly from, DateOnly to)
        => await _api.GetAsync<List<TopCropResult>>($"api/reports/top-crop?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}")
           ?? new();

    public async Task<List<FarmRevenueResult>> GetFarmRevenueAsync()
        => await _api.GetAsync<List<FarmRevenueResult>>("api/reports/farm-revenue") ?? new();
}