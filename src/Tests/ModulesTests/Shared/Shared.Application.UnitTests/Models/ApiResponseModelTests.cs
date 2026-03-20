using Shared.Application.Models;

namespace Shared.Application.UnitTests.Models;


public class ApiResponseModelTests
{
    /// <summary>
    /// Tests that Error method creates a response with Success=false, correct Message, and null Errors
    /// when errors parameter is not provided (uses default null value).
    /// </summary>
    [Fact]
    public void Error_WithMessageOnly_ReturnsErrorResponseWithNullErrors()
    {
        // Arrange
        string message = "An error occurred";

        // Act
        var result = ApiResponseModel<string>.Error(message);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Tests that Error method creates a response with Success=false, correct Message, and null Errors
    /// when errors parameter is explicitly set to null.
    /// </summary>
    [Fact]
    public void Error_WithMessageAndExplicitNullErrors_ReturnsErrorResponseWithNullErrors()
    {
        // Arrange
        string message = "An error occurred";

        // Act
        var result = ApiResponseModel<string>.Error(message, errors: null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Tests that Error method creates a response with Success=false, correct Message, and provided Errors
    /// when errors parameter is supplied.
    /// </summary>
    [Fact]
    public void Error_WithMessageAndErrors_ReturnsErrorResponseWithErrors()
    {
        // Arrange
        string message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error1", "Error2" } },
            { "Field2", new[] { "Error3" } }
        };

        // Act
        var result = ApiResponseModel<string>.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Tests that Error method correctly handles empty error dictionary.
    /// </summary>
    [Fact]
    public void Error_WithEmptyErrorsDictionary_ReturnsErrorResponseWithEmptyErrors()
    {
        // Arrange
        string message = "Error with empty dictionary";
        var errors = new Dictionary<string, string[]>();

        // Act
        var result = ApiResponseModel<object>.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Empty(result.Errors);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Tests that Error method correctly handles various message edge cases.
    /// Tests empty string, whitespace, very long string, and special characters.
    /// </summary>
    /// <param name="message">The message to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\r")]
    [InlineData("This is a very long error message that could potentially be used in real-world scenarios where detailed error information needs to be communicated to the client or logged for debugging purposes. It contains multiple sentences and goes on for quite a while to test the behavior with lengthy input.")]
    [InlineData("Special chars: \n\t\r\\\"'<>{}[]")]
    [InlineData("Unicode: 你好世界 🚀 émoji")]
    public void Error_WithVariousMessageEdgeCases_ReturnsErrorResponseWithCorrectMessage(string message)
    {
        // Arrange & Act
        var result = ApiResponseModel<string>.Error(message);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
        Assert.Null(result.Data);
    }

    /// <summary>
    /// Tests that Error method works correctly with different generic type parameters.
    /// Verifies that Data property is null regardless of the generic type.
    /// </summary>
    [Fact]
    public void Error_WithDifferentGenericTypes_DataIsAlwaysNull()
    {
        // Arrange
        string message = "Generic type test";

        // Act
        var stringResult = ApiResponseModel<string>.Error(message);
        var nullableIntResult = ApiResponseModel<int?>.Error(message);
        var objectResult = ApiResponseModel<object>.Error(message);

        // Assert
        Assert.Null(stringResult.Data);
        Assert.Null(nullableIntResult.Data);
        Assert.Null(objectResult.Data);
    }

    /// <summary>
    /// Tests that Error method correctly handles errors dictionary with empty string arrays.
    /// </summary>
    [Fact]
    public void Error_WithErrorsDictionaryContainingEmptyArrays_ReturnsErrorResponseWithEmptyArrays()
    {
        // Arrange
        string message = "Error with empty arrays";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new string[] { } },
            { "Field2", new string[] { } }
        };

        // Act
        var result = ApiResponseModel<string>.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.All(result.Errors.Values, array => Assert.Empty(array));
    }

    /// <summary>
    /// Tests that Error method correctly handles errors dictionary with single entry.
    /// </summary>
    [Fact]
    public void Error_WithSingleErrorEntry_ReturnsErrorResponseWithSingleEntry()
    {
        // Arrange
        string message = "Single error";
        var errors = new Dictionary<string, string[]>
        {
            { "Username", new[] { "Username is required" } }
        };

        // Act
        var result = ApiResponseModel<string>.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Username", result.Errors.Keys);
    }

    /// <summary>
    /// Tests that Error method correctly handles errors dictionary with multiple error messages per field.
    /// </summary>
    [Fact]
    public void Error_WithMultipleErrorsPerField_ReturnsErrorResponseWithAllErrors()
    {
        // Arrange
        string message = "Multiple validation errors";
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format is invalid", "Email already exists" } }
        };

        // Act
        var result = ApiResponseModel<string>.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Equal(3, result.Errors["Email"].Length);
    }

    /// <summary>
    /// Tests that Ok method returns a success response with the provided data and null message when message is not provided.
    /// </summary>
    [Fact]
    public void Ok_WithDataAndDefaultMessage_ReturnsSuccessResponseWithDataAndNullMessage()
    {
        // Arrange
        const string data = "test data";

        // Act
        var result = ApiResponseModel<string>.Ok(data);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method returns a success response with the provided data and message.
    /// </summary>
    [Fact]
    public void Ok_WithDataAndMessage_ReturnsSuccessResponseWithDataAndMessage()
    {
        // Arrange
        const string data = "test data";
        const string message = "Operation completed successfully";

        // Act
        var result = ApiResponseModel<string>.Ok(data, message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method correctly handles null data for reference types.
    /// </summary>
    [Fact]
    public void Ok_WithNullReferenceTypeData_ReturnsSuccessResponseWithNullData()
    {
        // Arrange
        string? data = null;
        const string message = "Success with null data";

        // Act
        var result = ApiResponseModel<string?>.Ok(data, message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method correctly handles value type data.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Ok_WithValueTypeData_ReturnsSuccessResponseWithCorrectValue(int data)
    {
        // Arrange
        const string message = "Value type success";

        // Act
        var result = ApiResponseModel<int>.Ok(data, message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method correctly handles various message edge cases.
    /// </summary>
    /// <param name="message">The message to test with.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Normal message")]
    [InlineData("Message with special characters: !@#$%^&*()")]
    [InlineData("Message\nwith\nnewlines")]
    [InlineData("Message\twith\ttabs")]
    public void Ok_WithVariousMessageValues_ReturnsSuccessResponseWithCorrectMessage(string? message)
    {
        // Arrange
        const int data = 123;

        // Act
        var result = ApiResponseModel<int>.Ok(data, message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method correctly handles very long message strings.
    /// </summary>
    [Fact]
    public void Ok_WithVeryLongMessage_ReturnsSuccessResponseWithCompleteMessage()
    {
        // Arrange
        const string data = "test";
        var longMessage = new string('a', 10000);

        // Act
        var result = ApiResponseModel<string>.Ok(data, longMessage);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(longMessage, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method correctly handles complex object types as data.
    /// </summary>
    [Fact]
    public void Ok_WithComplexObjectData_ReturnsSuccessResponseWithCorrectData()
    {
        // Arrange
        var complexData = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };
        const string message = "Complex object success";

        // Act
        var result = ApiResponseModel<Dictionary<string, int>>.Ok(complexData, message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(complexData, result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method correctly handles collection types with various states.
    /// </summary>
    [Fact]
    public void Ok_WithEmptyCollection_ReturnsSuccessResponseWithEmptyCollection()
    {
        // Arrange
        var emptyList = new List<string>();
        const string message = "Empty collection";

        // Act
        var result = ApiResponseModel<List<string>>.Ok(emptyList, message);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(emptyList, result.Data);
        Assert.Empty(result.Data);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Ok method always sets Errors property to null.
    /// </summary>
    [Fact]
    public void Ok_AlwaysSetsErrorsToNull()
    {
        // Arrange
        const string data = "test";
        const string message = "success";

        // Act
        var result = ApiResponseModel<string>.Ok(data, message);

        // Assert
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that the Ok method creates a successful response with the provided message.
    /// Verifies that Success is true, Message matches the input, and Errors is null.
    /// </summary>
    /// <param name="message">The message to pass to the Ok method</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Operation completed successfully")]
    [InlineData("Success with special chars: !@#$%^&*()")]
    [InlineData("Unicode: 你好世界 🎉")]
    [InlineData("Very long message that exceeds typical length to test handling of large strings in the response model which should work without any issues")]
    public void Ok_WithVariousMessages_CreatesSuccessfulResponseWithMessage(string? message)
    {
        // Act
        ApiResponseModel result = ApiResponseModel.Ok(message);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that the Ok method with no parameters (using default null message)
    /// creates a successful response with null message.
    /// </summary>
    [Fact]
    public void Ok_WithNoParameters_CreatesSuccessfulResponseWithNullMessage()
    {
        // Act
        ApiResponseModel result = ApiResponseModel.Ok();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that the Ok method with control characters in the message
    /// correctly preserves those characters.
    /// </summary>
    [Fact]
    public void Ok_WithControlCharacters_PreservesControlCharacters()
    {
        // Arrange
        string messageWithControlChars = "Line1\nLine2\rLine3\tTabbed";

        // Act
        ApiResponseModel result = ApiResponseModel.Ok(messageWithControlChars);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(messageWithControlChars, result.Message);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Error method returns a failure response with the provided message and null errors when errors parameter is not provided.
    /// Input: A non-empty message string.
    /// Expected: ApiResponseModel with Success=false, Message set to input, and Errors=null.
    /// </summary>
    [Fact]
    public void Error_WithMessageOnly_ReturnsFailureResponseWithNullErrors()
    {
        // Arrange
        string message = "An error occurred";

        // Act
        ApiResponseModel result = ApiResponseModel.Error(message);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Error method returns a failure response with the provided message and errors dictionary.
    /// Input: A message string and a dictionary of errors.
    /// Expected: ApiResponseModel with Success=false, Message set to input, and Errors set to input dictionary.
    /// </summary>
    [Fact]
    public void Error_WithMessageAndErrors_ReturnsFailureResponseWithErrors()
    {
        // Arrange
        string message = "Validation failed";
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", new[] { "Error1", "Error2" } },
            { "Field2", new[] { "Error3" } }
        };

        // Act
        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
    }

    /// <summary>
    /// Tests that Error method handles various message edge cases correctly.
    /// Input: Various edge case message strings (empty, whitespace, long, special characters).
    /// Expected: ApiResponseModel with Success=false and Message set to the input string.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    [InlineData("A very long error message that contains many characters and could potentially cause issues if there were length constraints in the implementation but since this is just a simple record it should handle it fine")]
    [InlineData("Special chars: !@#$%^&*()_+-={}[]|\\:\";<>?,./~`")]
    [InlineData("Unicode: 你好世界 🚀 ñ é")]
    [InlineData("Control chars: \r\n\t\0")]
    public void Error_WithVariousMessageEdgeCases_ReturnsFailureResponseWithMessage(string message)
    {
        // Arrange & Act
        ApiResponseModel result = ApiResponseModel.Error(message);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }

    /// <summary>
    /// Tests that Error method handles various errors dictionary edge cases correctly.
    /// Input: Various edge case error dictionaries (null, empty, single entry, multiple entries).
    /// Expected: ApiResponseModel with Success=false and Errors set to the input dictionary.
    /// </summary>
    [Fact]
    public void Error_WithEmptyErrorsDictionary_ReturnsFailureResponseWithEmptyErrors()
    {
        // Arrange
        string message = "Error";
        Dictionary<string, string[]> errors = new();

        // Act
        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that Error method handles errors dictionary with empty string arrays.
    /// Input: A message and a dictionary containing keys with empty string arrays.
    /// Expected: ApiResponseModel with Success=false and Errors preserving the empty arrays.
    /// </summary>
    [Fact]
    public void Error_WithErrorsDictionaryContainingEmptyArrays_ReturnsFailureResponse()
    {
        // Arrange
        string message = "Error";
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", Array.Empty<string>() },
            { "Field2", new string[] { } }
        };

        // Act
        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Equal(2, result.Errors!.Count);
    }

    /// <summary>
    /// Tests that Error method handles errors dictionary with special keys and values.
    /// Input: A message and a dictionary with edge case keys and values (empty strings, special characters).
    /// Expected: ApiResponseModel with Success=false and Errors preserving all entries.
    /// </summary>
    [Fact]
    public void Error_WithErrorsDictionaryWithSpecialKeysAndValues_ReturnsFailureResponse()
    {
        // Arrange
        string message = "Validation error";
        Dictionary<string, string[]> errors = new()
        {
            { "", new[] { "Empty key error" } },
            { "Normal", new[] { "", "  ", "valid error" } },
            { "Special!@#", new[] { "Special characters in key" } }
        };

        // Act
        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Equal(3, result.Errors!.Count);
    }

    /// <summary>
    /// Tests that Error method handles explicit null errors parameter.
    /// Input: A message and explicit null for errors parameter.
    /// Expected: ApiResponseModel with Success=false, Message set to input, and Errors=null.
    /// </summary>
    [Fact]
    public void Error_WithExplicitNullErrors_ReturnsFailureResponseWithNullErrors()
    {
        // Arrange
        string message = "Error occurred";

        // Act
        ApiResponseModel result = ApiResponseModel.Error(message, null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }
}