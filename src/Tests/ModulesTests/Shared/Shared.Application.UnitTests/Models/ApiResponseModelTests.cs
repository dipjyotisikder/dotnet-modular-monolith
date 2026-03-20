using Shared.Application.Models;

namespace Shared.Application.UnitTests.Models;

public class ApiResponseModelTests
{
    [Fact]
    public void Error_WithMessageOnly_ReturnsErrorResponseWithNullErrors()
    {
        string message = "An error occurred";

        var result = ApiResponseModel<string>.Error(message);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Error_WithMessageAndExplicitNullErrors_ReturnsErrorResponseWithNullErrors()
    {
        string message = "An error occurred";

        var result = ApiResponseModel<string>.Error(message, errors: null);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Error_WithMessageAndErrors_ReturnsErrorResponseWithErrors()
    {
        string message = "Validation failed";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error1", "Error2" } },
            { "Field2", new[] { "Error3" } }
        };

        var result = ApiResponseModel<string>.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Error_WithEmptyErrorsDictionary_ReturnsErrorResponseWithEmptyErrors()
    {
        string message = "Error with empty dictionary";
        var errors = new Dictionary<string, string[]>();

        var result = ApiResponseModel<object>.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Empty(result.Errors);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\r")]
    [InlineData("This is a very long error message that could potentially be used in real-world scenarios where detailed error information needs to be communicated to the client or logged for debugging purposes. It contains multiple sentences and goes on for quite a while to test the behavior with lengthy input.")]
    [InlineData("Special chars: \n\t\r\\\"'<>{}[]")]
    [InlineData("Unicode: 你好世界 🚀 émoji")]
    public void Error_WithVariousMessageEdgeCases_ReturnsErrorResponseWithCorrectMessage(string message)
    {
        var result = ApiResponseModel<string>.Error(message);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Error_WithDifferentGenericTypes_DataIsAlwaysNull()
    {
        string message = "Generic type test";

        var stringResult = ApiResponseModel<string>.Error(message);
        var nullableIntResult = ApiResponseModel<int?>.Error(message);
        var objectResult = ApiResponseModel<object>.Error(message);

        Assert.Null(stringResult.Data);
        Assert.Null(nullableIntResult.Data);
        Assert.Null(objectResult.Data);
    }

    [Fact]
    public void Error_WithErrorsDictionaryContainingEmptyArrays_ReturnsErrorResponseWithEmptyArrays()
    {
        string message = "Error with empty arrays";
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new string[] { } },
            { "Field2", new string[] { } }
        };

        var result = ApiResponseModel<string>.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.All(result.Errors.Values, array => Assert.Empty(array));
    }

    [Fact]
    public void Error_WithSingleErrorEntry_ReturnsErrorResponseWithSingleEntry()
    {
        string message = "Single error";
        var errors = new Dictionary<string, string[]>
        {
            { "Username", new[] { "Username is required" } }
        };

        var result = ApiResponseModel<string>.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Single(result.Errors);
        Assert.Contains("Username", result.Errors.Keys);
    }

    [Fact]
    public void Error_WithMultipleErrorsPerField_ReturnsErrorResponseWithAllErrors()
    {
        string message = "Multiple validation errors";
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format is invalid", "Email already exists" } }
        };

        var result = ApiResponseModel<string>.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Equal(3, result.Errors["Email"].Length);
    }

    [Fact]
    public void Ok_WithDataAndDefaultMessage_ReturnsSuccessResponseWithDataAndNullMessage()
    {
        const string data = "test data";

        var result = ApiResponseModel<string>.Ok(data);

        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithDataAndMessage_ReturnsSuccessResponseWithDataAndMessage()
    {
        const string data = "test data";
        const string message = "Operation completed successfully";

        var result = ApiResponseModel<string>.Ok(data, message);

        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithNullReferenceTypeData_ReturnsSuccessResponseWithNullData()
    {
        string? data = null;
        const string message = "Success with null data";

        var result = ApiResponseModel<string?>.Ok(data, message);

        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Ok_WithValueTypeData_ReturnsSuccessResponseWithCorrectValue(int data)
    {
        const string message = "Value type success";

        var result = ApiResponseModel<int>.Ok(data, message);

        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

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
        const int data = 123;

        var result = ApiResponseModel<int>.Ok(data, message);

        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithVeryLongMessage_ReturnsSuccessResponseWithCompleteMessage()
    {
        const string data = "test";
        var longMessage = new string('a', 10000);

        var result = ApiResponseModel<string>.Ok(data, longMessage);

        Assert.True(result.Success);
        Assert.Equal(longMessage, result.Message);
        Assert.Equal(data, result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithComplexObjectData_ReturnsSuccessResponseWithCorrectData()
    {
        var complexData = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 }
        };
        const string message = "Complex object success";

        var result = ApiResponseModel<Dictionary<string, int>>.Ok(complexData, message);

        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(complexData, result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithEmptyCollection_ReturnsSuccessResponseWithEmptyCollection()
    {
        var emptyList = new List<string>();
        const string message = "Empty collection";

        var result = ApiResponseModel<List<string>>.Ok(emptyList, message);

        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(emptyList, result.Data);
        Assert.Empty(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_AlwaysSetsErrorsToNull()
    {
        const string data = "test";
        const string message = "success";

        var result = ApiResponseModel<string>.Ok(data, message);

        Assert.Null(result.Errors);
    }

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
        ApiResponseModel result = ApiResponseModel.Ok(message);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithNoParameters_CreatesSuccessfulResponseWithNullMessage()
    {
        ApiResponseModel result = ApiResponseModel.Ok();

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Ok_WithControlCharacters_PreservesControlCharacters()
    {
        string messageWithControlChars = "Line1\nLine2\rLine3\tTabbed";

        ApiResponseModel result = ApiResponseModel.Ok(messageWithControlChars);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(messageWithControlChars, result.Message);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Error_WithMessageOnly_ReturnsFailureResponseWithNullErrors()
    {
        string message = "An error occurred";

        ApiResponseModel result = ApiResponseModel.Error(message);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Error_WithMessageAndErrors_ReturnsFailureResponseWithErrors()
    {
        string message = "Validation failed";
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", new[] { "Error1", "Error2" } },
            { "Field2", new[] { "Error3" } }
        };

        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
    }

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
        ApiResponseModel result = ApiResponseModel.Error(message);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }

    [Fact]
    public void Error_WithEmptyErrorsDictionary_ReturnsFailureResponseWithEmptyErrors()
    {
        string message = "Error";
        Dictionary<string, string[]> errors = new();

        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Error_WithErrorsDictionaryContainingEmptyArrays_ReturnsFailureResponse()
    {
        string message = "Error";
        Dictionary<string, string[]> errors = new()
        {
            { "Field1", Array.Empty<string>() },
            { "Field2", new string[] { } }
        };

        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Equal(2, result.Errors!.Count);
    }

    [Fact]
    public void Error_WithErrorsDictionaryWithSpecialKeysAndValues_ReturnsFailureResponse()
    {
        string message = "Validation error";
        Dictionary<string, string[]> errors = new()
        {
            { "", new[] { "Empty key error" } },
            { "Normal", new[] { "", "  ", "valid error" } },
            { "Special!@#", new[] { "Special characters in key" } }
        };

        ApiResponseModel result = ApiResponseModel.Error(message, errors);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Same(errors, result.Errors);
        Assert.Equal(3, result.Errors!.Count);
    }

    [Fact]
    public void Error_WithExplicitNullErrors_ReturnsFailureResponseWithNullErrors()
    {
        string message = "Error occurred";

        ApiResponseModel result = ApiResponseModel.Error(message, null);

        Assert.False(result.Success);
        Assert.Equal(message, result.Message);
        Assert.Null(result.Errors);
    }
}
