namespace OnlineShop.Modules.Ordering.Domain;

public interface IVoucherRepository
{
    Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Voucher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Voucher>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Voucher voucher);
}
