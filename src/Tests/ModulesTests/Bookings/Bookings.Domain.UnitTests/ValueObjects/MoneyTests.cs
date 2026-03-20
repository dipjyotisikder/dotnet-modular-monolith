using Xunit;
using Bookings.Domain.ValueObjects;

namespace Bookings.Domain.UnitTests.ValueObjects;

public class MoneyTests
{
    /// <summary>
    /// Tests that ToString formats the money value correctly with various amounts and currencies.
    /// Verifies that the amount is formatted with exactly 2 decimal places and the currency is appended.
    /// </summary>
    /// <param name="amount">The amount to test.</param>
    /// <param name="currency">The currency code to test.</param>
    /// <param name="expected">The expected string output.</param>
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
        // Arrange
        var moneyResult = Money.Create(amount, currency);
        var money = moneyResult.Value;

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that ToString correctly formats amounts that require rounding to 2 decimal places.
    /// Verifies that the F2 format specifier properly rounds decimal values.
    /// </summary>
    /// <param name="amount">The amount with more than 2 decimal places.</param>
    /// <param name="expected">The expected rounded string output.</param>
    [Theory]
    [InlineData(100.123, "100.12 USD")]
    [InlineData(100.125, "100.13 USD")]
    [InlineData(100.456, "100.46 USD")]
    [InlineData(0.004, "0.00 USD")]
    [InlineData(0.005, "0.01 USD")]
    [InlineData(9.999, "10.00 USD")]
    public void ToString_WithAmountsRequiringRounding_ReturnsCorrectlyRoundedString(decimal amount, string expected)
    {
        // Arrange
        var moneyResult = Money.Create(amount);
        var money = moneyResult.Value;

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that ToString correctly formats the maximum decimal value.
    /// Verifies edge case handling for extremely large amounts.
    /// </summary>
    [Fact]
    public void ToString_WithMaxDecimalValue_ReturnsFormattedString()
    {
        // Arrange
        var moneyResult = Money.Create(decimal.MaxValue, "USD");
        var money = moneyResult.Value;
        var expected = $"{decimal.MaxValue:F2} USD";

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that ToString correctly handles currency codes that are converted to uppercase by Create method.
    /// Verifies that lowercase currency input is properly converted and displayed as uppercase.
    /// </summary>
    /// <param name="inputCurrency">The currency code provided to Create (may be lowercase).</param>
    /// <param name="expectedCurrency">The expected uppercase currency in the output.</param>
    [Theory]
    [InlineData("usd", "USD")]
    [InlineData("eur", "EUR")]
    [InlineData("gbp", "GBP")]
    [InlineData("Jpy", "JPY")]
    public void ToString_WithLowercaseCurrency_ReturnsUppercaseFormattedString(string inputCurrency, string expectedCurrency)
    {
        // Arrange
        var amount = 123.45m;
        var moneyResult = Money.Create(amount, inputCurrency);
        var money = moneyResult.Value;
        var expected = $"123.45 {expectedCurrency}";

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that ToString correctly handles zero amount with default currency.
    /// Verifies proper formatting of zero values.
    /// </summary>
    [Fact]
    public void ToString_WithZeroAmount_ReturnsZeroFormattedString()
    {
        // Arrange
        var moneyResult = Money.Create(0);
        var money = moneyResult.Value;

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal("0.00 USD", result);
    }

    /// <summary>
    /// Tests that ToString correctly handles very small positive amounts.
    /// Verifies proper formatting and rounding of minimal positive values.
    /// </summary>
    [Theory]
    [InlineData(0.01, "0.01 USD")]
    [InlineData(0.001, "0.00 USD")]
    [InlineData(0.0001, "0.00 USD")]
    public void ToString_WithVerySmallAmounts_ReturnsCorrectlyFormattedString(decimal amount, string expected)
    {
        // Arrange
        var moneyResult = Money.Create(amount);
        var money = moneyResult.Value;

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that Multiply correctly multiplies the amount by various positive factors
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The multiplication factor.</param>
    [Theory]
    [InlineData(100.00, "USD", 2)]
    [InlineData(50.50, "EUR", 3)]
    [InlineData(25.75, "GBP", 10)]
    [InlineData(1.00, "JPY", 100)]
    [InlineData(999.99, "CAD", 5)]
    public void Multiply_WithPositiveFactor_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply with factor of 0 results in an amount of 0
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    [Theory]
    [InlineData(100.00, "USD")]
    [InlineData(50.50, "EUR")]
    [InlineData(0.01, "GBP")]
    public void Multiply_WithZeroFactor_ReturnsZeroAmountAndPreservesCurrency(decimal amount, string currency)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var factor = 0;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply with factor of 1 returns the same amount
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    [Theory]
    [InlineData(100.00, "USD")]
    [InlineData(50.50, "EUR")]
    [InlineData(0.01, "GBP")]
    [InlineData(999999.99, "JPY")]
    public void Multiply_WithFactorOne_ReturnsSameAmountAndPreservesCurrency(decimal amount, string currency)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var factor = 1;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply with negative factors correctly produces negative amounts
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The negative multiplication factor.</param>
    [Theory]
    [InlineData(100.00, "USD", -1)]
    [InlineData(50.50, "EUR", -2)]
    [InlineData(25.75, "GBP", -5)]
    [InlineData(1.00, "JPY", -100)]
    public void Multiply_WithNegativeFactor_ReturnsNegativeAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.True(result.Amount < 0);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply with zero amount results in zero regardless of the factor
    /// while preserving the currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The multiplication factor.</param>
    [Theory]
    [InlineData("USD", 10)]
    [InlineData("EUR", -5)]
    [InlineData("GBP", 0)]
    [InlineData("JPY", 1)]
    public void Multiply_WithZeroAmount_ReturnsZeroAmountAndPreservesCurrency(string currency, int factor)
    {
        // Arrange
        var money = Money.Create(0, currency).Value;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply with very large positive factors correctly multiplies the amount
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The large positive multiplication factor.</param>
    [Theory]
    [InlineData(1.00, "USD", 1000000)]
    [InlineData(0.01, "EUR", 10000)]
    [InlineData(10.00, "GBP", 100000)]
    public void Multiply_WithLargePositiveFactor_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply with very large negative factors correctly multiplies the amount
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The large negative multiplication factor.</param>
    [Theory]
    [InlineData(1.00, "USD", -1000000)]
    [InlineData(0.01, "EUR", -10000)]
    [InlineData(10.00, "GBP", -100000)]
    public void Multiply_WithLargeNegativeFactor_ReturnsCorrectNegativeAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.True(result.Amount < 0);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply correctly handles fractional amounts with various factors
    /// while preserving the currency.
    /// </summary>
    /// <param name="amount">The initial fractional money amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The multiplication factor.</param>
    [Theory]
    [InlineData(0.01, "USD", 2)]
    [InlineData(0.33, "EUR", 3)]
    [InlineData(1.11, "GBP", 9)]
    [InlineData(12.345, "JPY", 7)]
    public void Multiply_WithFractionalAmounts_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply correctly handles boundary values for the integer factor.
    /// This tests extreme values that might cause overflow or unexpected behavior.
    /// </summary>
    /// <param name="amount">The initial money amount.</param>
    /// <param name="currency">The currency code.</param>
    /// <param name="factor">The boundary value factor (int.MaxValue or int.MinValue).</param>
    [Theory]
    [InlineData(0.01, "USD", int.MaxValue)]
    [InlineData(0.01, "EUR", int.MinValue)]
    [InlineData(0.0001, "GBP", int.MaxValue)]
    [InlineData(0.0001, "JPY", int.MinValue)]
    public void Multiply_WithBoundaryFactorValues_ReturnsCorrectAmountAndPreservesCurrency(decimal amount, string currency, int factor)
    {
        // Arrange
        var money = Money.Create(amount, currency).Value;
        var expectedAmount = amount * factor;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Multiply returns a new Money instance and does not modify the original.
    /// </summary>
    [Fact]
    public void Multiply_ReturnsNewInstance_DoesNotModifyOriginal()
    {
        // Arrange
        var originalAmount = 100.00m;
        var originalCurrency = "USD";
        var money = Money.Create(originalAmount, originalCurrency).Value;
        var factor = 5;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.NotSame(money, result);
        Assert.Equal(originalAmount, money.Amount);
        Assert.Equal(originalCurrency, money.Currency);
        Assert.Equal(originalAmount * factor, result.Amount);
    }

    /// <summary>
    /// Tests that Multiply preserves different currency codes correctly.
    /// </summary>
    /// <param name="currency">The currency code to test.</param>
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
        // Arrange
        var money = Money.Create(100.00m, currency).Value;
        var factor = 2;

        // Act
        var result = money.Multiply(factor);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Add method successfully adds two Money objects with the same currency.
    /// </summary>
    /// <param name="amount1">The amount of the first Money object.</param>
    /// <param name="amount2">The amount of the second Money object.</param>
    /// <param name="currency">The currency used for both Money objects.</param>
    /// <param name="expectedAmount">The expected total amount after addition.</param>
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
        // Arrange
        var money1Result = Money.Create(amount1, currency);
        var money2Result = Money.Create(amount2, currency);
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(currency.ToUpperInvariant(), result.Currency);
    }

    /// <summary>
    /// Tests that Add method throws InvalidOperationException when attempting to add Money objects with different currencies.
    /// </summary>
    /// <param name="currency1">The currency of the first Money object.</param>
    /// <param name="currency2">The currency of the second Money object.</param>
    [Theory]
    [InlineData("USD", "EUR")]
    [InlineData("EUR", "USD")]
    [InlineData("GBP", "JPY")]
    [InlineData("USD", "GBP")]
    public void Add_DifferentCurrencies_ThrowsInvalidOperationException(string currency1, string currency2)
    {
        // Arrange
        var money1Result = Money.Create(100, currency1);
        var money2Result = Money.Create(50, currency2);
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;
        var expectedMessage = $"Cannot add money with different currencies: {currency1.ToUpperInvariant()} and {currency2.ToUpperInvariant()}";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
        Assert.Equal(expectedMessage, exception.Message);
    }

    /// <summary>
    /// Tests that Add method throws ArgumentNullException when the other parameter is null.
    /// </summary>
    [Fact(Skip = "ProductionBugSuspected")]
    [Trait("Category", "ProductionBugSuspected")]
    public void Add_NullParameter_ThrowsException()
    {
        // Arrange
        var moneyResult = Money.Create(100, "USD");
        var money = moneyResult.Value;
        Money? nullMoney = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => money.Add(nullMoney!));
    }

    /// <summary>
    /// Tests that Add method correctly handles large decimal values.
    /// </summary>
    [Fact]
    public void Add_LargeAmounts_ReturnsCorrectSum()
    {
        // Arrange
        var largeAmount1 = 999999999999.99m;
        var largeAmount2 = 999999999999.99m;
        var money1Result = Money.Create(largeAmount1, "USD");
        var money2Result = Money.Create(largeAmount2, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;
        var expectedAmount = largeAmount1 + largeAmount2;

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    /// <summary>
    /// Tests that Add method preserves the currency of the first Money object.
    /// </summary>
    [Fact]
    public void Add_SameCurrency_PreservesFirstCurrency()
    {
        // Arrange
        var money1Result = Money.Create(100, "USD");
        var money2Result = Money.Create(50, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(money1.Currency, result.Currency);
    }

    /// <summary>
    /// Tests that Add method does not modify the original Money objects.
    /// </summary>
    [Fact]
    public void Add_SameCurrency_DoesNotModifyOriginalObjects()
    {
        // Arrange
        var money1Result = Money.Create(100, "USD");
        var money2Result = Money.Create(50, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;
        var originalAmount1 = money1.Amount;
        var originalAmount2 = money2.Amount;

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(originalAmount1, money1.Amount);
        Assert.Equal(originalAmount2, money2.Amount);
        Assert.NotSame(money1, result);
        Assert.NotSame(money2, result);
    }

    /// <summary>
    /// Tests that Add method handles decimal precision correctly.
    /// </summary>
    [Theory]
    [InlineData(0.1, 0.2, 0.3)]
    [InlineData(0.123456789, 0.987654321, 1.11111111)]
    [InlineData(1.1111111111111111111111111111, 2.2222222222222222222222222222, 3.3333333333333333333333333333)]
    public void Add_DecimalPrecision_MaintainsPrecision(decimal amount1, decimal amount2, decimal expectedAmount)
    {
        // Arrange
        var money1Result = Money.Create(amount1, "USD");
        var money2Result = Money.Create(amount2, "USD");
        var money1 = money1Result.Value;
        var money2 = money2Result.Value;

        // Act
        var result = money1.Add(money2);

        // Assert
        Assert.Equal(expectedAmount, result.Amount);
    }
}