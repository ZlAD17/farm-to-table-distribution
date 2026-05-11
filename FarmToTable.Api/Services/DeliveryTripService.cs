using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class DeliveryTripService(DeliveryTripRepository repo)
{
    public Task<IReadOnlyList<DeliveryTripResponse>> GetAllAsync() => repo.GetAllAsync();

    public async Task<DeliveryTripResponse> GetByIdAsync(int id)
    {
        var trip = await repo.GetByIdAsync(id);
        return trip ?? throw new KeyNotFoundException($"DeliveryTrip {id} not found.");
    }

    public Task<DeliveryTripResponse> CreateAsync(CreateDeliveryTripRequest req)
        => repo.CreateAsync(req);

    public async Task<DeliveryTripResponse> CompleteAsync(int id, CompleteDeliveryTripRequest req)
    {
        var trip = await repo.CompleteAsync(id, req);
        return trip ?? throw new KeyNotFoundException($"DeliveryTrip {id} not found.");
    }
}
