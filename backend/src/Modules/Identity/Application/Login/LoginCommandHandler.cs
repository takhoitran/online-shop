using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Application.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Incorrect username or password.");

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Username username;
        try
        {
            username = Username.Create(request.Username);
        }
        catch (DomainException)
        {
            return Result.Failure<AuthResultDto>(InvalidCredentials);
        }

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResultDto>(InvalidCredentials);

        var role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure<AuthResultDto>(new Error("User.RoleNotFound", "This account has no valid role."));

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user, role.Name);

        return Result.Success(new AuthResultDto(user.Id, user.Username.Value, user.FullName, role.Name, token, expiresAtUtc));
    }
}
