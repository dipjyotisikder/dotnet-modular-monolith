using FluentValidation.TestHelper;
using Users.Features.UserManagement.CreateUser;

namespace Users.Features.UnitTests.UserManagement.CreateUser;

public class CreateUserCommandValidatorTests
{
    [Theory]
    [InlineData("user@example.com", "John")]
    [InlineData("test.user@domain.com", "Jane Doe")]
    [InlineData("valid@email.org", "AB")]
    [InlineData("another.email@test.co.uk", "Very Long Name With Multiple Words")]
    public void Constructor_ValidEmailAndName_ValidationPasses(string email, string name)
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(email, name);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Email is required")]
    [InlineData(" ", "Email is required")]
    [InlineData("   ", "Email is required")]
    [InlineData("invalid", "Email must be valid")]
    [InlineData("invalid@", "Email must be valid")]
    [InlineData("@invalid.com", "Email must be valid")]
    [InlineData("invalid@@example.com", "Email must be valid")]
    [InlineData("invalid..email@example.com", "Email must be valid")]
    [InlineData("invalid email@example.com", "Email must be valid")]
    public void Constructor_InvalidEmail_ValidationFailsWithCorrectMessage(string email, string expectedErrorMessage)
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(email, "ValidName");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(expectedErrorMessage);
    }

    [Theory]
    [InlineData("", "Name is required")]
    [InlineData(" ", "Name is required")]
    [InlineData("   ", "Name is required")]
    [InlineData("A", "Name must be at least 2 characters")]
    public void Constructor_InvalidName_ValidationFailsWithCorrectMessage(string name, string expectedErrorMessage)
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("valid@email.com", name);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(expectedErrorMessage);
    }

    [Fact]
    public void Constructor_InvalidEmailAndName_ValidationFailsWithMultipleErrors()
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("", "");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Constructor_NameWithExactlyTwoCharacters_ValidationPasses()
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("valid@email.com", "AB");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("user name@example.com")]
    [InlineData("user\t@example.com")]
    [InlineData("user\n@example.com")]
    [InlineData("user,name@example.com")]
    public void Constructor_EmailWithInvalidSpecialCharacters_ValidationFails(string email)
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(email, "ValidName");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }

    [Theory]
    [InlineData("O'Brien")]
    [InlineData("Mary-Jane")]
    [InlineData("José")]
    [InlineData("François")]
    [InlineData("名前")]
    public void Constructor_NameWithValidSpecialCharacters_ValidationPasses(string name)
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("valid@email.com", name);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Constructor_VeryLongValidInputs_ValidationPasses()
    {
        var validator = new CreateUserCommandValidator();
        var longEmail = new string('a', 50) + "@" + new string('b', 50) + ".com";
        var longName = new string('N', 1000);
        var command = new CreateUserCommand(longEmail, longName);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
