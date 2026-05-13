using FarmToTable.Gui.Models;

namespace FarmToTable.Gui.Services;

public class OrderGuiService
{
    private readonly ApiService _api;

    public OrderGuiService(ApiService api) => _api = api;

    public async Task<List<OrderResponse>> GetAllAsync()
        => await _api.GetAsync<List<OrderResponse>>("api/orders") ?? new();

    public async Task<OrderResponse> GetByIdAsync(int id)
        => await _api.GetAsync<OrderResponse>($"api/orders/{id}")
           ?? throw new Exception("الطلب غير موجود");

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest req)
        => await _api.PostAsync<CreateOrderRequest, OrderResponse>("api/orders", req)
           ?? throw new Exception("فشل في إنشاء الطلب");

    public async Task<OrderResponse> UpdateAsync(int id, UpdateOrderRequest req)
        => await _api.PutAsync<UpdateOrderRequest, OrderResponse>($"api/orders/{id}", req)
           ?? throw new Exception("فشل في تحديث الطلب");
}