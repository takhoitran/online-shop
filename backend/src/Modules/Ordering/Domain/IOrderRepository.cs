namespace OnlineShop.Modules.Ordering.Domain;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Order order);
}
