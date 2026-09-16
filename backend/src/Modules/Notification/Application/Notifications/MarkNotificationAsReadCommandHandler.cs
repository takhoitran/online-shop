using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Notification.Application.Abstractions;

namespace OnlineShop.Modules.Notification.Application.Notifications;

public sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IAppNotificationRepository _repository;
    private readonly INotificationUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(IAppNotificationRepository repository, INotificationUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _repository.GetByIdForUserAsync(request.NotificationId, request.UserId, cancellationToken);
        if (notification is null)
            return Result.Failure(new Error("Notification.NotFound", "Notification not found."));

        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
