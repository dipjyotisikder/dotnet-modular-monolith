using Xunit;
using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.ValueObjects;

public class AddressTests
{
    /// <summary>
    /// Tests that Create succeeds with valid inputs for all required parameters and returns a successful Result with properly trimmed values.
    /// </summary>
    [Fact]
    public void Create_ValidInputsWithoutZipCode_ReturnsSuccessWithTrimmedValues()
    {
        // Arrange
        var street = "  123 Main St  ";
        var city = "  New York  ";
        var country = "  USA  ";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("123 Main St", result.Value.Street);
        Assert.Equal("New York", result.Value.City);
        Assert.Equal("USA", result.Value.Country);
        Assert.Equal(string.Empty, result.Value.ZipCode);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// Tests that Create succeeds with valid inputs including zipCode and returns a successful Result with all values trimmed.
    /// </summary>
    [Fact]
    public void Create_ValidInputsWithZipCode_ReturnsSuccessWithTrimmedValues()
    {
        // Arrange
        var street = "  456 Oak Ave  ";
        var city = "  Los Angeles  ";
        var country = "  USA  ";
        var zipCode = "  90210  ";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("456 Oak Ave", result.Value.Street);
        Assert.Equal("Los Angeles", result.Value.City);
        Assert.Equal("USA", result.Value.Country);
        Assert.Equal("90210", result.Value.ZipCode);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// Tests that Create succeeds with empty zipCode explicitly provided.
    /// </summary>
    [Fact]
    public void Create_ValidInputsWithEmptyZipCode_ReturnsSuccess()
    {
        // Arrange
        var street = "123 Main St";
        var city = "Chicago";
        var country = "USA";
        var zipCode = "";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that Create succeeds with whitespace-only zipCode and trims it to empty.
    /// </summary>
    [Fact]
    public void Create_ValidInputsWithWhitespaceZipCode_ReturnsSuccessWithEmptyZipCode()
    {
        // Arrange
        var street = "123 Main St";
        var city = "Chicago";
        var country = "USA";
        var zipCode = "   ";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that Create fails with appropriate error when street is null.
    /// </summary>
    [Fact]
    public void Create_NullStreet_ReturnsFailureWithValidationError()
    {
        // Arrange
        string street = null!;
        var city = "New York";
        var country = "USA";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails with appropriate error when street is empty string.
    /// </summary>
    [Fact]
    public void Create_EmptyStreet_ReturnsFailureWithValidationError()
    {
        // Arrange
        var street = "";
        var city = "New York";
        var country = "USA";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when street contains only whitespace characters.
    /// </summary>
    /// <param name="whitespaceStreet">Whitespace-only street value to test.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void Create_WhitespaceStreet_ReturnsFailureWithValidationError(string whitespaceStreet)
    {
        // Arrange
        var city = "New York";
        var country = "USA";

        // Act
        var result = Address.Create(whitespaceStreet, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails with appropriate error when city is null.
    /// </summary>
    [Fact]
    public void Create_NullCity_ReturnsFailureWithValidationError()
    {
        // Arrange
        var street = "123 Main St";
        string city = null!;
        var country = "USA";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("City cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails with appropriate error when city is empty string.
    /// </summary>
    [Fact]
    public void Create_EmptyCity_ReturnsFailureWithValidationError()
    {
        // Arrange
        var street = "123 Main St";
        var city = "";
        var country = "USA";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("City cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when city contains only whitespace characters.
    /// </summary>
    /// <param name="whitespaceCity">Whitespace-only city value to test.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void Create_WhitespaceCity_ReturnsFailureWithValidationError(string whitespaceCity)
    {
        // Arrange
        var street = "123 Main St";
        var country = "USA";

        // Act
        var result = Address.Create(street, whitespaceCity, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("City cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails with appropriate error when country is null.
    /// </summary>
    [Fact]
    public void Create_NullCountry_ReturnsFailureWithValidationError()
    {
        // Arrange
        var street = "123 Main St";
        var city = "New York";
        string country = null!;

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Country cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails with appropriate error when country is empty string.
    /// </summary>
    [Fact]
    public void Create_EmptyCountry_ReturnsFailureWithValidationError()
    {
        // Arrange
        var street = "123 Main St";
        var city = "New York";
        var country = "";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Country cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when country contains only whitespace characters.
    /// </summary>
    /// <param name="whitespaceCountry">Whitespace-only country value to test.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void Create_WhitespaceCountry_ReturnsFailureWithValidationError(string whitespaceCountry)
    {
        // Arrange
        var street = "123 Main St";
        var city = "New York";

        // Act
        var result = Address.Create(street, city, whitespaceCountry);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Country cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns the first validation error (street) when multiple parameters are invalid.
    /// </summary>
    [Fact]
    public void Create_MultipleInvalidParameters_ReturnsFirstValidationError()
    {
        // Arrange
        var street = "";
        var city = "";
        var country = "";

        // Act
        var result = Address.Create(street, city, country);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create succeeds with very long string inputs.
    /// </summary>
    [Fact]
    public void Create_VeryLongStrings_ReturnsSuccess()
    {
        // Arrange
        var street = new string('A', 10000);
        var city = new string('B', 10000);
        var country = new string('C', 10000);
        var zipCode = new string('D', 10000);

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(country, result.Value.Country);
        Assert.Equal(zipCode, result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that Create succeeds with special characters in all fields.
    /// </summary>
    [Fact]
    public void Create_SpecialCharacters_ReturnsSuccess()
    {
        // Arrange
        var street = "123 Main St. !@#$%^&*()";
        var city = "São Paulo";
        var country = "Côte d'Ivoire";
        var zipCode = "12345-6789";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(country, result.Value.Country);
        Assert.Equal(zipCode, result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that Create succeeds with Unicode characters in all fields.
    /// </summary>
    [Fact]
    public void Create_UnicodeCharacters_ReturnsSuccess()
    {
        // Arrange
        var street = "北京路123号";
        var city = "東京";
        var country = "日本";
        var zipCode = "〒100-0001";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(country, result.Value.Country);
        Assert.Equal(zipCode, result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that Create properly trims leading and trailing whitespace from all fields.
    /// </summary>
    [Fact]
    public void Create_InputsWithMixedWhitespace_ProperlyTrimsAllFields()
    {
        // Arrange
        var street = "\t  123 Main St  \n";
        var city = "\r\n  New York  \t";
        var country = "  \t USA \n  ";
        var zipCode = "\n 10001 \t";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St", result.Value.Street);
        Assert.Equal("New York", result.Value.City);
        Assert.Equal("USA", result.Value.Country);
        Assert.Equal("10001", result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that Create succeeds with single character strings.
    /// </summary>
    [Fact]
    public void Create_SingleCharacterInputs_ReturnsSuccess()
    {
        // Arrange
        var street = "A";
        var city = "B";
        var country = "C";
        var zipCode = "D";

        // Act
        var result = Address.Create(street, city, country, zipCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("A", result.Value.Street);
        Assert.Equal("B", result.Value.City);
        Assert.Equal("C", result.Value.Country);
        Assert.Equal("D", result.Value.ZipCode);
    }

    /// <summary>
    /// Tests that ToString formats the address correctly with all properties populated including ZipCode.
    /// Verifies the format: "Street, City, Country ZipCode".
    /// </summary>
    [Theory]
    [InlineData("123 Main St", "New York", "USA", "10001", "123 Main St, New York, USA 10001")]
    [InlineData("456 Oak Ave", "Los Angeles", "USA", "90001", "456 Oak Ave, Los Angeles, USA 90001")]
    [InlineData("1", "A", "B", "C", "1, A, B C")]
    public void ToString_WithAllPropertiesIncludingZipCode_ReturnsFormattedAddressString(
        string street, string city, string country, string zipCode, string expected)
    {
        // Arrange
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString correctly handles empty ZipCode by trimming trailing whitespace.
    /// Verifies that when ZipCode is empty, the format is "Street, City, Country" without trailing space.
    /// </summary>
    [Theory]
    [InlineData("123 Main St", "New York", "USA", "", "123 Main St, New York, USA")]
    [InlineData("456 Oak Ave", "Los Angeles", "USA", "", "456 Oak Ave, Los Angeles, USA")]
    [InlineData("Baker Street", "London", "UK", "", "Baker Street, London, UK")]
    public void ToString_WithEmptyZipCode_ReturnsFormattedAddressWithoutTrailingSpace(
        string street, string city, string country, string zipCode, string expected)
    {
        // Arrange
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString correctly handles properties with special characters.
    /// Verifies that special characters (apostrophes, diacritics, hyphens) are preserved in the output.
    /// </summary>
    [Theory]
    [InlineData("O'Connell Street", "Dublin", "Ireland", "D01", "O'Connell Street, Dublin, Ireland D01")]
    [InlineData("Rue de l'Église", "Paris", "France", "75001", "Rue de l'Église, Paris, France 75001")]
    [InlineData("São João Avenue", "São Paulo", "Brazil", "01000-000", "São João Avenue, São Paulo, Brazil 01000-000")]
    [InlineData("Main St", "Zürich", "Switzerland", "8001", "Main St, Zürich, Switzerland 8001")]
    public void ToString_WithSpecialCharacters_PreservesSpecialCharacters(
        string street, string city, string country, string zipCode, string expected)
    {
        // Arrange
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString correctly handles properties containing commas.
    /// Verifies that commas within property values are preserved and distinguished from format commas.
    /// </summary>
    [Theory]
    [InlineData("123 Main St, Apt 4", "New York", "USA", "10001", "123 Main St, Apt 4, New York, USA 10001")]
    [InlineData("Building A, Floor 2", "City, District", "Country", "12345", "Building A, Floor 2, City, District, Country 12345")]
    public void ToString_WithCommasInProperties_PreservesCommasInData(
        string street, string city, string country, string zipCode, string expected)
    {
        // Arrange
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString correctly handles very long property values.
    /// Verifies that long strings are not truncated and are formatted correctly.
    /// </summary>
    [Fact]
    public void ToString_WithVeryLongPropertyValues_ReturnsCompleteFormattedString()
    {
        // Arrange
        var longStreet = new string('A', 200);
        var longCity = new string('B', 150);
        var longCountry = new string('C', 100);
        var longZipCode = new string('D', 50);
        var expected = $"{longStreet}, {longCity}, {longCountry} {longZipCode}";

        var result = Address.Create(longStreet, longCity, longCountry, longZipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString correctly handles properties with whitespace that gets trimmed by Create.
    /// Verifies that the Create method trims values and ToString formats the trimmed values correctly.
    /// </summary>
    [Theory]
    [InlineData("  123 Main St  ", "  New York  ", "  USA  ", "  10001  ", "123 Main St, New York, USA 10001")]
    [InlineData("\tTabbed Street\t", "\tTabbed City\t", "\tTabbed Country\t", "\tTabbed Zip\t", "Tabbed Street, Tabbed City, Tabbed Country Tabbed Zip")]
    public void ToString_WithLeadingAndTrailingWhitespace_FormatsTrimedValues(
        string street, string city, string country, string zipCode, string expected)
    {
        // Arrange
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString handles properties with numbers and mixed alphanumeric characters.
    /// Verifies correct formatting with various alphanumeric combinations.
    /// </summary>
    [Theory]
    [InlineData("123", "456", "789", "000", "123, 456, 789 000")]
    [InlineData("Street123", "City456", "Country789", "Zip000", "Street123, City456, Country789 Zip000")]
    [InlineData("1st Avenue", "2nd District", "3rd Region", "4th-5678", "1st Avenue, 2nd District, 3rd Region 4th-5678")]
    public void ToString_WithNumericAndAlphanumericValues_ReturnsFormattedString(
        string street, string city, string country, string zipCode, string expected)
    {
        // Arrange
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests that ToString handles single character values for all properties.
    /// Verifies correct formatting with minimal-length property values.
    /// </summary>
    [Fact]
    public void ToString_WithSingleCharacterValues_ReturnsFormattedString()
    {
        // Arrange
        var result = Address.Create("A", "B", "C", "D");
        var address = result.Value;

        // Act
        var actual = address.ToString();

        // Assert
        Assert.Equal("A, B, C D", actual);
    }
}