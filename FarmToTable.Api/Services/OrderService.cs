using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class OrderService(OrderRepository repo)
{
    public Task<IReadOnlyList<OrderResponse>> GetAllAsync() => repo.GetAllAsync();

    public async Task<OrderResponse> GetByIdAsync(int id)
    {
        var order = await repo.GetByIdAsync(id);
        return order ?? throw new KeyNotFoundException($"Order {id} not found.");
    }

    public Task<OrderResponse> CreateAsync(CreateOrderRequest req) => repo.CreateAsync(req);

    public async Task<OrderResponse> UpdateAsync(int id, UpdateOrderRequest req)
    {
        var validStatuses = new[] { "Pending", "Confirmed", "Delivered", "Cancelled" };
        if (!validStatuses.Contains(req.Status))
            throw new ArgumentException($"Invalid status '{req.Status}'.");

        var order = await repo.UpdateAsync(id, req);
        return order ?? throw new KeyNotFoundException($"Order {id} not found.");
    }
}
