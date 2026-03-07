using Shared.Domain;

namespace Bookings.Domain.ValueObjects;

public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "USD";

    private Money() { }

    public static Result<Money> Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            return Result.Failure<Money>("Amount cannot be negative", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>("Currency cannot be empty", ErrorCodes.VALIDATION_ERROR);

        return Result.Success(new Money
        {
            Amount = amount,
            Currency = currency.ToUpperInvariant()
        });
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money { Amount = Amount + other.Amount, Currency = Currency };
    }

    public Money Multiply(int factor) =>
        new() { Amount = Amount * factor, Currency = Currency };

    public override string ToString() => $"{Amount:F2} {Currency}";
}
