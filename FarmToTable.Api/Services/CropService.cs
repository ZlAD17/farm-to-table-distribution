using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class CropService(CropRepository repo)
{
    public Task<IReadOnlyList<CropResponse>> GetAllAsync() => repo.GetAllAsync();

    public Task<CropResponse> CreateAsync(CreateCropRequest req) => repo.CreateAsync(req);
}
