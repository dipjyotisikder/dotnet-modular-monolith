using Bookings.Domain.ValueObjects;
using Shared.Domain;

namespace Bookings.Domain.UnitTests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_ValidInputsWithoutZipCode_ReturnsSuccessWithTrimmedValues()
    {
        var street = "  123 Main St  ";
        var city = "  New York  ";
        var country = "  USA  ";

        var result = Address.Create(street, city, country);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("123 Main St", result.Value.Street);
        Assert.Equal("New York", result.Value.City);
        Assert.Equal("USA", result.Value.Country);
        Assert.Equal(string.Empty, result.Value.ZipCode);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Create_ValidInputsWithZipCode_ReturnsSuccessWithTrimmedValues()
    {
        var street = "  456 Oak Ave  ";
        var city = "  Los Angeles  ";
        var country = "  USA  ";
        var zipCode = "  90210  ";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal("456 Oak Ave", result.Value.Street);
        Assert.Equal("Los Angeles", result.Value.City);
        Assert.Equal("USA", result.Value.Country);
        Assert.Equal("90210", result.Value.ZipCode);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Create_ValidInputsWithEmptyZipCode_ReturnsSuccess()
    {
        var street = "123 Main St";
        var city = "Chicago";
        var country = "USA";
        var zipCode = "";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.ZipCode);
    }

    [Fact]
    public void Create_ValidInputsWithWhitespaceZipCode_ReturnsSuccessWithEmptyZipCode()
    {
        var street = "123 Main St";
        var city = "Chicago";
        var country = "USA";
        var zipCode = "   ";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.ZipCode);
    }

    [Fact]
    public void Create_NullStreet_ReturnsFailureWithValidationError()
    {
        string street = null!;
        var city = "New York";
        var country = "USA";

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_EmptyStreet_ReturnsFailureWithValidationError()
    {
        var street = "";
        var city = "New York";
        var country = "USA";

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void Create_WhitespaceStreet_ReturnsFailureWithValidationError(string whitespaceStreet)
    {
        var city = "New York";
        var country = "USA";

        var result = Address.Create(whitespaceStreet, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_NullCity_ReturnsFailureWithValidationError()
    {
        var street = "123 Main St";
        string city = null!;
        var country = "USA";

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("City cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_EmptyCity_ReturnsFailureWithValidationError()
    {
        var street = "123 Main St";
        var city = "";
        var country = "USA";

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("City cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void Create_WhitespaceCity_ReturnsFailureWithValidationError(string whitespaceCity)
    {
        var street = "123 Main St";
        var country = "USA";

        var result = Address.Create(street, whitespaceCity, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("City cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_NullCountry_ReturnsFailureWithValidationError()
    {
        var street = "123 Main St";
        var city = "New York";
        string country = null!;

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Country cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_EmptyCountry_ReturnsFailureWithValidationError()
    {
        var street = "123 Main St";
        var city = "New York";
        var country = "";

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Country cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void Create_WhitespaceCountry_ReturnsFailureWithValidationError(string whitespaceCountry)
    {
        var street = "123 Main St";
        var city = "New York";

        var result = Address.Create(street, city, whitespaceCountry);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Country cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_MultipleInvalidParameters_ReturnsFirstValidationError()
    {
        var street = "";
        var city = "";
        var country = "";

        var result = Address.Create(street, city, country);

        Assert.False(result.IsSuccess);
        Assert.Equal("Street cannot be empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public void Create_VeryLongStrings_ReturnsSuccess()
    {
        var street = new string('A', 10000);
        var city = new string('B', 10000);
        var country = new string('C', 10000);
        var zipCode = new string('D', 10000);

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(country, result.Value.Country);
        Assert.Equal(zipCode, result.Value.ZipCode);
    }

    [Fact]
    public void Create_SpecialCharacters_ReturnsSuccess()
    {
        var street = "123 Main St. !@#$%^&*()";
        var city = "São Paulo";
        var country = "Côte d'Ivoire";
        var zipCode = "12345-6789";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(country, result.Value.Country);
        Assert.Equal(zipCode, result.Value.ZipCode);
    }

    [Fact]
    public void Create_UnicodeCharacters_ReturnsSuccess()
    {
        var street = "北京路123号";
        var city = "東京";
        var country = "日本";
        var zipCode = "〒100-0001";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal(street, result.Value.Street);
        Assert.Equal(city, result.Value.City);
        Assert.Equal(country, result.Value.Country);
        Assert.Equal(zipCode, result.Value.ZipCode);
    }

    [Fact]
    public void Create_InputsWithMixedWhitespace_ProperlyTrimsAllFields()
    {
        var street = "\t  123 Main St  \n";
        var city = "\r\n  New York  \t";
        var country = "  \t USA \n  ";
        var zipCode = "\n 10001 \t";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal("123 Main St", result.Value.Street);
        Assert.Equal("New York", result.Value.City);
        Assert.Equal("USA", result.Value.Country);
        Assert.Equal("10001", result.Value.ZipCode);
    }

    [Fact]
    public void Create_SingleCharacterInputs_ReturnsSuccess()
    {
        var street = "A";
        var city = "B";
        var country = "C";
        var zipCode = "D";

        var result = Address.Create(street, city, country, zipCode);

        Assert.True(result.IsSuccess);
        Assert.Equal("A", result.Value.Street);
        Assert.Equal("B", result.Value.City);
        Assert.Equal("C", result.Value.Country);
        Assert.Equal("D", result.Value.ZipCode);
    }

    [Theory]
    [InlineData("123 Main St", "New York", "USA", "10001", "123 Main St, New York, USA 10001")]
    [InlineData("456 Oak Ave", "Los Angeles", "USA", "90001", "456 Oak Ave, Los Angeles, USA 90001")]
    [InlineData("1", "A", "B", "C", "1, A, B C")]
    public void ToString_WithAllPropertiesIncludingZipCode_ReturnsFormattedAddressString(
        string street, string city, string country, string zipCode, string expected)
    {
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123 Main St", "New York", "USA", "", "123 Main St, New York, USA")]
    [InlineData("456 Oak Ave", "Los Angeles", "USA", "", "456 Oak Ave, Los Angeles, USA")]
    [InlineData("Baker Street", "London", "UK", "", "Baker Street, London, UK")]
    public void ToString_WithEmptyZipCode_ReturnsFormattedAddressWithoutTrailingSpace(
        string street, string city, string country, string zipCode, string expected)
    {
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("O'Connell Street", "Dublin", "Ireland", "D01", "O'Connell Street, Dublin, Ireland D01")]
    [InlineData("Rue de l'Église", "Paris", "France", "75001", "Rue de l'Église, Paris, France 75001")]
    [InlineData("São João Avenue", "São Paulo", "Brazil", "01000-000", "São João Avenue, São Paulo, Brazil 01000-000")]
    [InlineData("Main St", "Zürich", "Switzerland", "8001", "Main St, Zürich, Switzerland 8001")]
    public void ToString_WithSpecialCharacters_PreservesSpecialCharacters(
        string street, string city, string country, string zipCode, string expected)
    {
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123 Main St, Apt 4", "New York", "USA", "10001", "123 Main St, Apt 4, New York, USA 10001")]
    [InlineData("Building A, Floor 2", "City, District", "Country", "12345", "Building A, Floor 2, City, District, Country 12345")]
    public void ToString_WithCommasInProperties_PreservesCommasInData(
        string street, string city, string country, string zipCode, string expected)
    {
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToString_WithVeryLongPropertyValues_ReturnsCompleteFormattedString()
    {
        var longStreet = new string('A', 200);
        var longCity = new string('B', 150);
        var longCountry = new string('C', 100);
        var longZipCode = new string('D', 50);
        var expected = $"{longStreet}, {longCity}, {longCountry} {longZipCode}";

        var result = Address.Create(longStreet, longCity, longCountry, longZipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("  123 Main St  ", "  New York  ", "  USA  ", "  10001  ", "123 Main St, New York, USA 10001")]
    [InlineData("\tTabbed Street\t", "\tTabbed City\t", "\tTabbed Country\t", "\tTabbed Zip\t", "Tabbed Street, Tabbed City, Tabbed Country Tabbed Zip")]
    public void ToString_WithLeadingAndTrailingWhitespace_FormatsTrimedValues(
        string street, string city, string country, string zipCode, string expected)
    {
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123", "456", "789", "000", "123, 456, 789 000")]
    [InlineData("Street123", "City456", "Country789", "Zip000", "Street123, City456, Country789 Zip000")]
    [InlineData("1st Avenue", "2nd District", "3rd Region", "4th-5678", "1st Avenue, 2nd District, 3rd Region 4th-5678")]
    public void ToString_WithNumericAndAlphanumericValues_ReturnsFormattedString(
        string street, string city, string country, string zipCode, string expected)
    {
        var result = Address.Create(street, city, country, zipCode);
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToString_WithSingleCharacterValues_ReturnsFormattedString()
    {
        var result = Address.Create("A", "B", "C", "D");
        var address = result.Value;

        var actual = address.ToString();

        Assert.Equal("A, B, C D", actual);
    }
}
