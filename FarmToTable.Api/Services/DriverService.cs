using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class DriverService(DriverRepository repo)
{
    public Task<IReadOnlyList<DriverResponse>> GetAllAsync() => repo.GetAllAsync();

    public async Task<DriverResponse> GetByIdAsync(int id)
    {
        var driver = await repo.GetByIdAsync(id);
        return driver ?? throw new KeyNotFoundException($"Driver {id} not found.");
    }

    public Task<DriverResponse> CreateAsync(CreateDriverRequest req) => repo.CreateAsync(req);

    public async Task<DriverResponse> UpdateAsync(int id, UpdateDriverRequest req)
    {
        var driver = await repo.UpdateAsync(id, req);
        return driver ?? throw new KeyNotFoundException($"Driver {id} not found.");
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted) throw new KeyNotFoundException($"Driver {id} not found.");
    }
}
