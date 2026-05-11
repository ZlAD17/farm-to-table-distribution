using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class RestaurantService(RestaurantRepository repo)
{
    public Task<IReadOnlyList<RestaurantResponse>> GetAllAsync() => repo.GetAllAsync();

    public async Task<RestaurantResponse> GetByIdAsync(int id)
    {
        var restaurant = await repo.GetByIdAsync(id);
        return restaurant ?? throw new KeyNotFoundException($"Restaurant {id} not found.");
    }

    public Task<RestaurantResponse> CreateAsync(CreateRestaurantRequest req) => repo.CreateAsync(req);

    public async Task<RestaurantResponse> UpdateAsync(int id, UpdateRestaurantRequest req)
    {
        var restaurant = await repo.UpdateAsync(id, req);
        return restaurant ?? throw new KeyNotFoundException($"Restaurant {id} not found.");
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted) throw new KeyNotFoundException($"Restaurant {id} not found.");
    }
}
