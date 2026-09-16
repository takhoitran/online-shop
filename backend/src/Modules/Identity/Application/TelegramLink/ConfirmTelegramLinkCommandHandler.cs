using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Application.TelegramLink;

public sealed class ConfirmTelegramLinkCommandHandler : IRequestHandler<ConfirmTelegramLinkCommand, Result>
{
    private readonly ITelegramLinkCodeRepository _linkCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ConfirmTelegramLinkCommandHandler(
        ITelegramLinkCodeRepository linkCodeRepository,
        IUserRepository userRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _linkCodeRepository = linkCodeRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ConfirmTelegramLinkCommand request, CancellationToken cancellationToken)
    {
        var linkCode = await _linkCodeRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (linkCode is null)
            return Result.Failure(new Error("TelegramLink.CodeNotFound", "This link code does not exist."));

        try
        {
            linkCode.Consume();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("TelegramLink.Invalid", ex.Message));
        }

        var user = await _userRepository.GetByIdAsync(linkCode.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(new Error("User.NotFound", "User to link was not found."));

        user.LinkTelegramAccount(request.TelegramId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
