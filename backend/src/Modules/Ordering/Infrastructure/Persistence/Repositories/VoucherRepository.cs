using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class VoucherRepository : IVoucherRepository
{
    private readonly OrderingDbContext _dbContext;

    public VoucherRepository(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Voucher?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _dbContext.Vouchers.SingleOrDefaultAsync(v => v.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<Voucher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Vouchers.SingleOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<List<Voucher>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Vouchers.AsNoTracking().OrderByDescending(v => v.CreatedAtUtc).ToListAsync(cancellationToken);

    public void Add(Voucher voucher) => _dbContext.Vouchers.Add(voucher);
}
