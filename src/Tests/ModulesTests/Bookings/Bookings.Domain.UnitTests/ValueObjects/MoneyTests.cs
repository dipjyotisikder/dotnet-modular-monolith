using Bookings.Domain.ValueObjects;

namespace Bookings.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    [Theory]
    [InlineData(0, "USD", "0.00 USD")]
    [InlineData(1.5, "USD", "1.50 USD")]
    [InlineData(100, "EUR", "100.00 EUR")]
    [InlineData(999999.99, "GBP", "999999.99 GBP")]
    [InlineData(0.01, "JPY", "0.01 JPY")]
    [InlineData(12345.67, "CHF", "12345.67 CHF")]
    [InlineData(0.001, "USD", "0.00 USD")]
    public void ToString_WithVariousAmountsAndCurrencies_ReturnsFormattedString(decimal amount, string currency, string expected)
    {
        var moneyResult = Money.Create(amount, currency);
        var money = moneyResult.Value;

        var result = money.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100.123, "100.12 USD")]
    [InlineData(100.125, "100.13 USD")]
    [InlineData(100.456, "100.46 USD")]
    [InlineData(0.004, "0.00 USD")]
    [InlineData(0.005, "0.01 USD")]
    [InlineData(9.999, "10.00 USD")]
    public void ToString_WithAmountsRequiringRounding_ReturnsCorrectlyRoundedString(decimal amount, string expected)
    {
        var moneyResult = Money.Create(amount);
        var money = moneyResult.Value;

        var result = money.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_WithMaxDecimalValue_ReturnsFormattedString()
    {
        var moneyResult = Money.Create(decimal.MaxValue, "USD");
        var money = moneyResult.Value;
        var expected = $"{decimal.MaxValue:F2} USD";

        var result = money.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("eur", "EUR")]
    [InlineData("gbp", "GBP")]
    [InlineData("Jpy", "JPY")]
    public void ToString_WithLowercaseCurrency_ReturnsUppercaseFormattedString(string inputCurrency, string expectedCurrency)
    {
        var amount = 123.45m;
        var moneyResult = Money.Create(amount, inputCurrency);
        var money = moneyResult.Value;
        var expected = $"123.45 {expectedCurrency}";

        var result = money.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_WithZeroAmount_ReturnsZeroFormattedString()
    {
        var moneyResult = Money.Create(0);
        var money = moneyResult.Value;

        var result = money.ToString();

        Assert.Equal("0.00 USD", result);
    }

    [Theory]
    [InlineData(0.01, "0.01 USD")]
    [InlineData(0.001, "0.00 USD")]
    [InlineData(0.0001, "0.00 USD")]
    public void ToString_WithVerySmallAmounts_ReturnsCorrectlyFormattedString(decimal amount, string expected)
    {
        var moneyResult = Money.Create(amount);
        var money = moneyResult.Value;

        var result = money.ToString();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(100.00, "USD", 2)]
    [InlineData(50.50, "EUR", 3)]
    [InlineData(25.75, "GBP", 10)]
    [InlineData(1.00, "JPY", 100)]
    [InlineData(999.99, "CAD", 5)]
    public void Multiply_WithPositiveFactor_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(100.00, "USD")]
    [InlineData(50.50, "EUR")]
    [InlineData(0.01, "GBP")]
    public void Multiply_WithZeroFactor_ReturnsZeroAmountAndPreservesCurrency(decimal amount, string currency)
    {
        var money = Money.Create(amount, currency).Value;
        var factor = 0;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(0, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(100.00, "USD")]
    [InlineData(50.50, "EUR")]
    [InlineData(0.01, "GBP")]
    [InlineData(999999.99, "JPY")]
    public void Multiply_WithFactorOne_ReturnsSameAmountAndPreservesCurrency(decimal amount, string currency)
    {
        var money = Money.Create(amount, currency).Value;
        var factor = 1;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(100.00, "USD", -1)]
    [InlineData(50.50, "EUR", -2)]
    [InlineData(25.75, "GBP", -5)]
    [InlineData(1.00, "JPY", -100)]
    public void Multiply_WithNegativeFactor_ReturnsNegativeAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.True(result.Amount < 0);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData("USD", 10)]
    [InlineData("EUR", -5)]
    [InlineData("GBP", 0)]
    [InlineData("JPY", 1)]
    public void Multiply_WithZeroAmount_ReturnsZeroAmountAndPreservesCurrency(string currency, int factor)
    {
        var money = Money.Create(0, currency).Value;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(0, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(1.00, "USD", 1000000)]
    [InlineData(0.01, "EUR", 10000)]
    [InlineData(10.00, "GBP", 100000)]
    public void Multiply_WithLargePositiveFactor_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(1.00, "USD", -1000000)]
    [InlineData(0.01, "EUR", -10000)]
    [InlineData(10.00, "GBP", -100000)]
    public void Multiply_WithLargeNegativeFactor_ReturnsCorrectNegativeAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.True(result.Amount < 0);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(0.01, "USD", 2)]
    [InlineData(0.33, "EUR", 3)]
    [InlineData(1.11, "GBP", 9)]
    [InlineData(12.345, "JPY", 7)]
    public void Multiply_WithFractionalAmounts_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(0.01, "USD", int.MaxValue)]
    [InlineData(0.01, "EUR", int.MinValue)]
    [InlineData(0.0001, "GBP", int.MaxValue)]
    [InlineData(0.0001, "JPY", int.MinValue)]
    public void Multiply_WithBoundaryFactorValues_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Fact]
    public void Multiply_ReturnsNewInstance_DoesNotModifyOriginal()
    {
        var originalAmount = 100.00m;
        var originalCurrency = "USD";
        var money = Money.Create(originalAmount, originalCurrency).Value;
        var factor = 5;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.NotSame(money, result);
        Assert.Equal(originalAmount, money.Amount);
        Assert.Equal(originalCurrency, money.Currency);
        Assert.Equal(originalAmount * factor, result.Amount);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("CAD")]
    [InlineData("aud")]
    [InlineData("chf")]
    public void Multiply_WithVariousCurrencies_PreservesCurrencyCorrectly(string currency)
    {
        var money = Money.Create(100.00m, currency).Value;
        var factor = 2;

        var result = money.Multiply(factor);

        Assert.NotNull(result);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData(100, 50, "USD", 150)]
    [InlineData(0, 100, "USD", 100)]
    [InlineData(100, 0, "USD", 100)]
    [InlineData(0, 0, "USD", 0)]
    [InlineData(99.99, 0.01, "USD", 100.00)]
    [InlineData(1.123456, 2.654321, "USD", 3.777777)]
    [InlineData(100, 50, "EUR", 150)]
    [InlineData(100, 50, "GBP", 150)]
    [InlineData(1000000, 2000000, "JPY", 3000000)]
    public void Add_SameCurrency_ReturnsCorrectSum(decimal amount1, decimal amount2, string currency, decimal expectedAmount)
    {
        var money1Result = Money.Create(amount1, currency);
        var money2Result = Money.Create(amount2, currency);
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;

        var result = money1.Add(money2);

        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    [Theory]
    [InlineData("USD", "EUR")]
    [InlineData("EUR", "USD")]
    [InlineData("GBP", "JPY")]
    [InlineData("USD", "GBP")]
    public void Add_DifferentCurrencies_ThrowsInvalidOperationException(string currency1, string currency2)
    {
        var money1Result = Money.Create(100, currency1);
        var money2Result = Money.Create(50, currency2);
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;
        var expectedMessage = $"Cannot add money with different currencies: {currency1.ToUpperInvariant()} and {currency2.ToUpperInvariant()}";

        var exception = Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Add_LargeAmounts_ReturnsCorrectSum()
    {
        var largeAmount1 = 999999999999.99m;
        var largeAmount2 = 999999999999.99m;
        var money1Result = Money.Create(largeAmount1, "USD");
        var money2Result = Money.Create(largeAmount2, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;
        var expectedAmount = largeAmount1 + largeAmount2;

        var result = money1.Add(money2);

        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Add_SameCurrency_PreservesFirstCurrency()
    {
        var money1Result = Money.Create(100, "USD");
        var money2Result = Money.Create(50, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;

        var result = money1.Add(money2);

        Assert.Equal(money1.Currency, result.Currency);
    }

    [Fact]
    public void Add_SameCurrency_DoesNotModifyOriginalObjects()
    {
        var money1Result = Money.Create(100, "USD");
        var money2Result = Money.Create(50, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;
        var originalAmount1 = money1.Amount;
        var originalAmount2 = money2.Amount;

        var result = money1.Add(money2);

        Assert.Equal(originalAmount1, money1.Amount);
        Assert.Equal(originalAmount2, money2.Amount);
        Assert.NotSame(money1, result);
        Assert.NotSame(money2, result);
    }

    [Theory]
    [InlineData(0.1, 0.2, 0.3)]
    [InlineData(0.123456789, 0.987654321, 1.11111111)]
    [InlineData(1.1111111111111111111111111111, 2.2222222222222222222222222222, 3.3333333333333333333333333333)]
    public void Add_DecimalPrecision_MaintainsPrecision(decimal amount1, decimal amount2, decimal expectedAmount)
    {
        var money1Result = Money.Create(amount1, "USD");
        var money2Result = Money.Create(amount2, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;

        var result = money1.Add(money2);

        Assert.Equal(expectedAmount, result.Amount);
    }
}
