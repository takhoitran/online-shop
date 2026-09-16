namespace OnlineShop.BuildingBlocks.Domain;

/// <summary>Thrown when an operation would violate a domain invariant.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
