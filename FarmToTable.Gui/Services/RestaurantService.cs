using FarmToTable.Gui.Models;

namespace FarmToTable.Gui.Services;

public class RestaurantGuiService
{
    private readonly ApiService _api;

    public RestaurantGuiService(ApiService api) => _api = api;

    public async Task<List<RestaurantResponse>> GetAllAsync()
        => await _api.GetAsync<List<RestaurantResponse>>("api/restaurants") ?? new();

    public async Task<RestaurantResponse> CreateAsync(CreateRestaurantRequest req)
        => await _api.PostAsync<CreateRestaurantRequest, RestaurantResponse>("api/restaurants", req)
           ?? throw new Exception("فشل في إنشاء المطعم");

    public async Task<RestaurantResponse> UpdateAsync(int id, UpdateRestaurantRequest req)
        => await _api.PutAsync<UpdateRestaurantRequest, RestaurantResponse>($"api/restaurants/{id}", req)
           ?? throw new Exception("فشل في تحديث المطعم");

    public async Task DeleteAsync(int id)
        => await _api.DeleteAsync($"api/restaurants/{id}");
}