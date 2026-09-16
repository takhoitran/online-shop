using MediatR;
using OnlineShop.Modules.Notification.Application.Abstractions;

namespace OnlineShop.Modules.Notification.Application.Notifications;

public sealed class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly INotificationQueries _queries;

    public GetUnreadNotificationCountQueryHandler(INotificationQueries queries)
    {
        _queries = queries;
    }

    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken) =>
        _queries.GetUnreadCountAsync(request.UserId, cancellationToken);
}
