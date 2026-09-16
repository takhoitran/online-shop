using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Application.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResultDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResultDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var username = Username.Create(request.Username);

        if (await _userRepository.ExistsByUsernameAsync(username, cancellationToken))
            return Result.Failure<AuthResultDto>(new Error("User.UsernameTaken", "Username is already taken."));

        var role = await _roleRepository.GetByNameAsync(RoleNames.Buyer, cancellationToken);
        if (role is null)
            return Result.Failure<AuthResultDto>(new Error("User.RoleNotFound", $"Role '{RoleNames.Buyer}' does not exist."));

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Register(username, passwordHash, request.FullName, role.Id);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user, role.Name);

        return Result.Success(new AuthResultDto(user.Id, user.Username.Value, user.FullName, role.Name, token, expiresAtUtc));
    }
}
