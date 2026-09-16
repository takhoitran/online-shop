using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Application.TelegramLink;

public sealed class GenerateTelegramLinkCodeCommandHandler
    : IRequestHandler<GenerateTelegramLinkCodeCommand, Result<TelegramLinkCodeDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITelegramLinkCodeRepository _linkCodeRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public GenerateTelegramLinkCodeCommandHandler(
        IUserRepository userRepository,
        ITelegramLinkCodeRepository linkCodeRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _linkCodeRepository = linkCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TelegramLinkCodeDto>> Handle(
        GenerateTelegramLinkCodeCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<TelegramLinkCodeDto>(new Error("User.NotFound", "User not found."));

        var linkCode = TelegramLinkCode.Generate(user.Id);
        _linkCodeRepository.Add(linkCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TelegramLinkCodeDto(linkCode.Code, linkCode.ExpiresAtUtc));
    }
}
