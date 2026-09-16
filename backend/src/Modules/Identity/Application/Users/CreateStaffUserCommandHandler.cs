using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Application.Users;

public sealed class CreateStaffUserCommandHandler : IRequestHandler<CreateStaffUserCommand, Result<UserSummaryDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreateStaffUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserSummaryDto>> Handle(CreateStaffUserCommand request, CancellationToken cancellationToken)
    {
        var username = Username.Create(request.Username);

        if (await _userRepository.ExistsByUsernameAsync(username, cancellationToken))
            return Result.Failure<UserSummaryDto>(new Error("User.UsernameTaken", "Username is already taken."));

        var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
        if (role is null)
            return Result.Failure<UserSummaryDto>(new Error("User.RoleNotFound", $"Role '{request.RoleName}' does not exist."));

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Register(username, passwordHash, request.FullName, role.Id);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserSummaryDto(user.Id, user.Username.Value, user.FullName, role.Name, false, user.CreatedAtUtc));
    }
}
