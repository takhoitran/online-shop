using MediatR;
using OnlineShop.Modules.Identity.Application.Abstractions;

namespace OnlineShop.Modules.Identity.Application.Users;

public sealed class GetUserTelegramIdQueryHandler : IRequestHandler<GetUserTelegramIdQuery, string?>
{
    private readonly IUserDirectoryQueries _queries;

    public GetUserTelegramIdQueryHandler(IUserDirectoryQueries queries)
    {
        _queries = queries;
    }

    public Task<string?> Handle(GetUserTelegramIdQuery request, CancellationToken cancellationToken) =>
        _queries.GetTelegramIdAsync(request.UserId, cancellationToken);
}
