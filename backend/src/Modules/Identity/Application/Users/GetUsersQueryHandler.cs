using MediatR;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Users;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserSummaryDto>>
{
    private readonly IUserDirectoryQueries _queries;

    public GetUsersQueryHandler(IUserDirectoryQueries queries)
    {
        _queries = queries;
    }

    public Task<List<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _queries.GetAllAsync(cancellationToken);
}
