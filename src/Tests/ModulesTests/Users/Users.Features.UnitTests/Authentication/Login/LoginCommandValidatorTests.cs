using FluentValidation.TestHelper;
using Users.Features.Authentication.Login;

namespace Users.Features.UnitTests.Authentication.Login;

public class LoginCommandValidatorTests
{
    [Fact]
    public void Constructor_WhenCalled_CreatesValidatorInstance()
    {
        var validator = new LoginCommandValidator();

        Assert.NotNull(validator);
    }

    [Theory]
    [InlineData("test@example.com", "password123")]
    [InlineData("user.name@domain.co.uk", "123456")]
    [InlineData("valid+email@test.com", "verylongpassword")]
    [InlineData("a@b.c", "minimumpass")]
    public void Constructor_ValidEmailAndPassword_ValidationPasses(string email, string password)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, password);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Constructor_NullEmail_ValidationFailsWithExpectedMessage()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(null!, "validpassword");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Constructor_EmptyEmail_ValidationFailsWithExpectedMessage()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(string.Empty, "validpassword");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_WhitespaceEmail_ValidationFails(string email)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, "validpassword");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("no-at-sign.com")]
    [InlineData("double@@domain.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("email@")]
    [InlineData("email@.com")]
    public void Constructor_InvalidEmailFormat_ValidationFailsWithExpectedMessage(string invalidEmail)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(invalidEmail, "validpassword");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }

    [Fact]
    public void Constructor_NullPassword_ValidationFailsWithExpectedMessage()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", null!);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Fact]
    public void Constructor_EmptyPassword_ValidationFailsWithExpectedMessage()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", string.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_WhitespacePassword_ValidationFails(string password)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", password);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("12345")]
    public void Constructor_PasswordLessThanSixCharacters_ValidationFailsWithExpectedMessage(string shortPassword)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", shortPassword);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters");
    }

    [Fact]
    public void Constructor_PasswordExactlySixCharacters_ValidationPasses()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", "123456");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("averylongpassword")]
    [InlineData("P@ssw0rd!")]
    public void Constructor_PasswordMoreThanSixCharacters_ValidationPasses(string password)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", password);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Constructor_BothEmailAndPasswordInvalid_ValidationFailsWithMultipleErrors()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(string.Empty, "123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Constructor_VeryLongValidEmail_ValidationPasses()
    {
        var validator = new LoginCommandValidator();
        var longEmail = new string('a', 50) + "@" + new string('b', 50) + ".com";
        var command = new LoginCommand(longEmail, "validpassword");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Constructor_VeryLongPassword_ValidationPasses()
    {
        var validator = new LoginCommandValidator();
        var longPassword = new string('x', 1000);
        var command = new LoginCommand("valid@email.com", longPassword);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("user+tag@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user_name@example.com")]
    [InlineData("123@example.com")]
    public void Constructor_EmailWithSpecialCharacters_ValidationPasses(string email)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, "validpassword");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("P@ssw0rd!")]
    [InlineData("p@$$w0rd")]
    [InlineData("pass word")]
    [InlineData("pass\tword")]
    public void Constructor_PasswordWithSpecialCharacters_ValidationPasses(string password)
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", password);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
