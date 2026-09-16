using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Identity.Application.Abstractions;

/// <summary>
/// Module-scoped Unit of Work. Every module defines its own — sharing the plain BuildingBlocks
/// IUnitOfWork across modules for DI resolution would let the *last* module registered silently
/// win for every handler in the process, since the built-in container doesn't key by module.
/// </summary>
public interface IIdentityUnitOfWork : IUnitOfWork
{
}
