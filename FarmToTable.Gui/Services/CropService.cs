using FarmToTable.Gui.Models;

namespace FarmToTable.Gui.Services;

public class CropGuiService
{
    private readonly ApiService _api;

    public CropGuiService(ApiService api) => _api = api;

    public async Task<List<CropResponse>> GetAllAsync()
        => await _api.GetAsync<List<CropResponse>>("api/crops") ?? new();

    public async Task<CropResponse> CreateAsync(CreateCropRequest req)
        => await _api.PostAsync<CreateCropRequest, CropResponse>("api/crops", req)
           ?? throw new Exception("فشل في إنشاء المحصول");
}