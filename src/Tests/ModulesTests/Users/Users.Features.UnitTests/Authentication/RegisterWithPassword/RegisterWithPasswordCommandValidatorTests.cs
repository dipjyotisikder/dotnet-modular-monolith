using FluentValidation.TestHelper;
using Users.Features.Authentication.RegisterWithPassword;

namespace Users.Features.UnitTests.Authentication.RegisterWithPassword;

public class RegisterWithPasswordCommandValidatorTests
{
    [Theory]
    [InlineData("user@example.com", "John", "Password123")]
    [InlineData("test.email+tag@domain.co.uk", "AB", "Abcdefg1")]
    [InlineData("very.long.email.address@subdomain.example.com", "Very Long Name With Multiple Words", "ComplexP@ssw0rd123")]
    [InlineData("user123@test.org", "名前", "MyP@ss123")]
    public void Constructor_ValidCommand_PassesValidation(string email, string name, string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(email, name, password);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Constructor_EmptyEmail_FailsValidation()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(string.Empty, "John", "Password123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WhitespaceEmail_FailsValidation(string email)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(email, "John", "Password123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("no@domain@double.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("user@")]
    public void Constructor_InvalidEmailFormat_FailsValidation(string invalidEmail)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(invalidEmail, "John", "Password123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }

    [Fact]
    public void Constructor_EmptyName_FailsValidation()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", string.Empty, "Password123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WhitespaceName_FailsValidation(string name)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", name, "Password123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Theory]
    [InlineData("A")]
    [InlineData("1")]
    public void Constructor_NameTooShort_FailsValidation(string name)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", name, "Password123");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must be at least 2 characters");
    }

    [Fact]
    public void Constructor_NameExactlyTwoCharacters_PassesValidation()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "AB", "Password123");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Constructor_EmptyPassword_FailsValidation()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", string.Empty);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WhitespacePassword_FailsValidation(string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData("Pass1")]
    [InlineData("Abc123")]
    [InlineData("Pass12A")]
    public void Constructor_PasswordTooShort_FailsValidation(string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }

    [Fact]
    public void Constructor_PasswordExactlyEightCharacters_PassesValidation()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", "Abcdef12");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("abcdefgh1")]
    [InlineData("mypassword999")]
    public void Constructor_PasswordMissingUppercase_FailsValidation(string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain uppercase letters");
    }

    [Theory]
    [InlineData("PASSWORD123")]
    [InlineData("ABCDEFGH1")]
    [InlineData("MYPASSWORD999")]
    public void Constructor_PasswordMissingLowercase_FailsValidation(string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain lowercase letters");
    }

    [Theory]
    [InlineData("Password")]
    [InlineData("Abcdefgh")]
    [InlineData("MyPassword")]
    public void Constructor_PasswordMissingNumbers_FailsValidation(string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain numbers");
    }

    [Fact]
    public void Constructor_PasswordMissingMultipleRequirements_FailsWithMultipleErrors()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", "password");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain uppercase letters");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain numbers");
    }

    [Fact]
    public void Constructor_MultipleFieldsInvalid_FailsWithMultipleErrors()
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(string.Empty, "A", "pass");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("P@ssw0rd")]
    [InlineData("MyP@ss123!")]
    [InlineData("Complex#Pass1")]
    public void Constructor_PasswordWithSpecialCharacters_PassesValidation(string password)
    {
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
