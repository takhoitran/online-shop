using FluentValidation;
using MediatR;

namespace OnlineShop.BuildingBlocks.Application;

/// <summary>
/// Runs every registered FluentValidation validator for TRequest before the handler executes.
/// Registered once per module (see each module's DependencyInjection extension) so Command/Query
/// handlers never have to call validators manually.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToList();

            if (failures.Count > 0)
            {
                var message = string.Join(" | ", failures.Select(f => f.ErrorMessage));
                throw new ValidationException(message, failures);
            }
        }

        return await next();
    }
}
