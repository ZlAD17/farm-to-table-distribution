using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class FarmService(FarmRepository repo)
{
    public Task<IReadOnlyList<FarmResponse>> GetAllAsync() => repo.GetAllAsync();

    public async Task<FarmResponse> GetByIdAsync(int id)
    {
        var farm = await repo.GetByIdAsync(id);
        return farm ?? throw new KeyNotFoundException($"Farm {id} not found.");
    }

    public Task<FarmResponse> CreateAsync(CreateFarmRequest req) => repo.CreateAsync(req);

    public async Task<FarmResponse> UpdateAsync(int id, UpdateFarmRequest req)
    {
        var farm = await repo.UpdateAsync(id, req);
        return farm ?? throw new KeyNotFoundException($"Farm {id} not found.");
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted) throw new KeyNotFoundException($"Farm {id} not found.");
    }
}
