using MediatR;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Users;

public sealed class GetUserByTelegramIdQueryHandler : IRequestHandler<GetUserByTelegramIdQuery, UserSummaryDto?>
{
    private readonly IUserDirectoryQueries _queries;

    public GetUserByTelegramIdQueryHandler(IUserDirectoryQueries queries)
    {
        _queries = queries;
    }

    public Task<UserSummaryDto?> Handle(GetUserByTelegramIdQuery request, CancellationToken cancellationToken) =>
        _queries.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
}
