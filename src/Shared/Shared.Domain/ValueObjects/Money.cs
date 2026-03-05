namespace Shared.Domain.ValueObjects;

public sealed record Money : IEquatable<Money>, IComparable<Money>
{
    public decimal Amount { get; }

    public static readonly Money Zero = new(0);

    public Money(decimal amount)
    {
        Amount = ValidateAmount(amount);
    }

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount Cannot Be Negative", ErrorCodes.VALIDATION_ERROR);

        return Result.Success(new Money(amount));
    }

    public Money Add(Money other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (other.Amount > Amount)
            throw new InvalidOperationException("Cannot Subtract Larger Amount");

        return new Money(Amount - other.Amount);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity Cannot Be Negative", nameof(quantity));

        return new Money(Amount * quantity);
    }

    public int CompareTo(Money? other)
    {
        if (other == null) return 1;
        return Amount.CompareTo(other.Amount);
    }

    public bool Equals(Money? other) => other is not null && Amount == other.Amount;

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => Amount.ToString("C");

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount Cannot Be Negative", nameof(amount));

        return amount;
    }
}
