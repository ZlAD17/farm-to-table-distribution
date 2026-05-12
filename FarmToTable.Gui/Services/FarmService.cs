using FarmToTable.Gui.Models;

namespace FarmToTable.Gui.Services;

public class FarmGuiService
{
    private readonly ApiService _api;

    public FarmGuiService(ApiService api) => _api = api;

    public async Task<List<FarmResponse>> GetAllAsync()
        => await _api.GetAsync<List<FarmResponse>>("api/farms") ?? new();

    public async Task<FarmResponse> CreateAsync(CreateFarmRequest req)
        => await _api.PostAsync<CreateFarmRequest, FarmResponse>("api/farms", req)
           ?? throw new Exception("فشل في إنشاء المزرعة");

    public async Task<FarmResponse> UpdateAsync(int id, UpdateFarmRequest req)
        => await _api.PutAsync<UpdateFarmRequest, FarmResponse>($"api/farms/{id}", req)
           ?? throw new Exception("فشل في تحديث المزرعة");

    public async Task DeleteAsync(int id)
        => await _api.DeleteAsync($"api/farms/{id}");
}