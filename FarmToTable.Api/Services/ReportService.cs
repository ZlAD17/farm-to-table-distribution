using FarmToTable.Api.Repositories;

namespace FarmToTable.Api.Services;

public class ReportService(ReportRepository repo)
{
    public Task<IReadOnlyList<TopCropResult>> GetTopCropAsync(DateOnly from, DateOnly to)
        => repo.GetTopCropByOrdersAsync(from, to);

    public Task<IReadOnlyList<InactiveFarmResult>> GetInactiveFarmsAsync(int year, int month)
        => repo.GetInactiveFarmsAsync(year, month);

    public Task<IReadOnlyList<TopDriverResult>> GetTopDriverAsync(int year, int month)
        => repo.GetTopDriverAsync(year, month);

    public Task<IReadOnlyList<InactiveRestaurantResult>> GetInactiveRestaurantsAsync(int year, int month)
        => repo.GetInactiveRestaurantsAsync(year, month);

    public Task<IReadOnlyList<RestaurantBatchResult>> GetBatchesDeliveredPerRestaurantAsync(int year, int month)
        => repo.GetBatchesDeliveredPerRestaurantAsync(year, month);

    public Task<IReadOnlyList<FarmRevenueResult>> GetFarmRevenueAsync()
        => repo.GetFarmRevenueAsync();
}
