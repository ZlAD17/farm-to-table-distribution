using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class HarvestBatchService(HarvestBatchRepository repo)
{
    public Task<IReadOnlyList<HarvestBatchResponse>> GetAllAsync(HarvestBatchFilterRequest filter)
        => repo.GetAllAsync(filter);

    public async Task<HarvestBatchResponse> GetByIdAsync(int id)
    {
        var batch = await repo.GetByIdAsync(id);
        return batch ?? throw new KeyNotFoundException($"HarvestBatch {id} not found.");
    }

    public Task<HarvestBatchResponse> CreateAsync(CreateHarvestBatchRequest req)
        => repo.CreateAsync(req);

    public async Task<HarvestBatchResponse> UpdateAsync(int id, UpdateHarvestBatchRequest req)
    {
        var batch = await repo.UpdateAsync(id, req);
        return batch ?? throw new KeyNotFoundException($"HarvestBatch {id} not found.");
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted) throw new KeyNotFoundException($"HarvestBatch {id} not found.");
    }
}
