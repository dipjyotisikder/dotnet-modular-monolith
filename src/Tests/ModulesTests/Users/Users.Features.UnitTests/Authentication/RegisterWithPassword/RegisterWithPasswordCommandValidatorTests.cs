using FluentValidation.TestHelper;
using Users.Features.Authentication.RegisterWithPassword;

namespace Users.Features.UnitTests.Authentication.RegisterWithPassword;

/// <summary>
/// Unit tests for <see cref="RegisterWithPasswordCommandValidator"/>.
/// </summary>
public class RegisterWithPasswordCommandValidatorTests
{
    /// <summary>
    /// Tests that the validator passes validation for valid commands with various acceptable inputs.
    /// </summary>
    /// <param name="email">The email address to test.</param>
    /// <param name="name">The name to test.</param>
    /// <param name="password">The password to test.</param>
    [Theory]
    [InlineData("user@example.com", "John", "Password123")]
    [InlineData("test.email+tag@domain.co.uk", "AB", "Abcdefg1")]
    [InlineData("very.long.email.address@subdomain.example.com", "Very Long Name With Multiple Words", "ComplexP@ssw0rd123")]
    [InlineData("user123@test.org", "名前", "MyP@ss123")]
    public void Constructor_ValidCommand_PassesValidation(string email, string name, string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(email, name, password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that the validator fails validation when email is empty.
    /// Expected error message: "Email is required"
    /// </summary>
    [Fact]
    public void Constructor_EmptyEmail_FailsValidation()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(string.Empty, "John", "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    /// <summary>
    /// Tests that the validator fails validation when email is whitespace only.
    /// Expected error message: "Email is required"
    /// </summary>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WhitespaceEmail_FailsValidation(string email)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(email, "John", "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    /// <summary>
    /// Tests that the validator fails validation when email format is invalid.
    /// Expected error message: "Email must be valid"
    /// </summary>
    /// <param name="invalidEmail">The invalid email format to test.</param>
    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("no@domain@double.com")]
    [InlineData("spaces in@email.com")]
    [InlineData("user@")]
    public void Constructor_InvalidEmailFormat_FailsValidation(string invalidEmail)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(invalidEmail, "John", "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }

    /// <summary>
    /// Tests that the validator fails validation when name is empty.
    /// Expected error message: "Name is required"
    /// </summary>
    [Fact]
    public void Constructor_EmptyName_FailsValidation()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", string.Empty, "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    /// <summary>
    /// Tests that the validator fails validation when name is whitespace only.
    /// Expected error message: "Name is required"
    /// </summary>
    /// <param name="name">The whitespace name to test.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WhitespaceName_FailsValidation(string name)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", name, "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    /// <summary>
    /// Tests that the validator fails validation when name is shorter than minimum length of 2 characters.
    /// Expected error message: "Name must be at least 2 characters"
    /// </summary>
    [Theory]
    [InlineData("A")]
    [InlineData("1")]
    public void Constructor_NameTooShort_FailsValidation(string name)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", name, "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must be at least 2 characters");
    }

    /// <summary>
    /// Tests that the validator passes validation when name is exactly 2 characters (boundary condition).
    /// </summary>
    [Fact]
    public void Constructor_NameExactlyTwoCharacters_PassesValidation()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "AB", "Password123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Tests that the validator fails validation when password is empty.
    /// Expected error message: "Password is required"
    /// </summary>
    [Fact]
    public void Constructor_EmptyPassword_FailsValidation()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", string.Empty);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    /// <summary>
    /// Tests that the validator fails validation when password is whitespace only.
    /// Expected error message: "Password is required"
    /// </summary>
    /// <param name="password">The whitespace password to test.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WhitespacePassword_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    /// <summary>
    /// Tests that the validator fails validation when password is shorter than minimum length of 8 characters.
    /// Expected error message: "Password must be at least 8 characters"
    /// </summary>
    /// <param name="password">The short password to test.</param>
    [Theory]
    [InlineData("Pass1")]
    [InlineData("Abc123")]
    [InlineData("Pass12A")]
    public void Constructor_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }

    /// <summary>
    /// Tests that the validator passes validation when password is exactly 8 characters with all requirements (boundary condition).
    /// </summary>
    [Fact]
    public void Constructor_PasswordExactlyEightCharacters_PassesValidation()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", "Abcdef12");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that the validator fails validation when password does not contain uppercase letters.
    /// Expected error message: "Password must contain uppercase letters"
    /// </summary>
    /// <param name="password">The password without uppercase letters to test.</param>
    [Theory]
    [InlineData("password123")]
    [InlineData("abcdefgh1")]
    [InlineData("mypassword999")]
    public void Constructor_PasswordMissingUppercase_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain uppercase letters");
    }

    /// <summary>
    /// Tests that the validator fails validation when password does not contain lowercase letters.
    /// Expected error message: "Password must contain lowercase letters"
    /// </summary>
    /// <param name="password">The password without lowercase letters to test.</param>
    [Theory]
    [InlineData("PASSWORD123")]
    [InlineData("ABCDEFGH1")]
    [InlineData("MYPASSWORD999")]
    public void Constructor_PasswordMissingLowercase_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain lowercase letters");
    }

    /// <summary>
    /// Tests that the validator fails validation when password does not contain numbers.
    /// Expected error message: "Password must contain numbers"
    /// </summary>
    /// <param name="password">The password without numbers to test.</param>
    [Theory]
    [InlineData("Password")]
    [InlineData("Abcdefgh")]
    [InlineData("MyPassword")]
    public void Constructor_PasswordMissingNumbers_FailsValidation(string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain numbers");
    }

    /// <summary>
    /// Tests that the validator correctly reports multiple validation errors when password fails multiple requirements.
    /// </summary>
    [Fact]
    public void Constructor_PasswordMissingMultipleRequirements_FailsWithMultipleErrors()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", "password");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain uppercase letters");
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain numbers");
    }

    /// <summary>
    /// Tests that the validator correctly reports multiple validation errors across different properties.
    /// </summary>
    [Fact]
    public void Constructor_MultipleFieldsInvalid_FailsWithMultipleErrors()
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand(string.Empty, "A", "pass");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that the validator allows passwords with special characters when all other requirements are met.
    /// </summary>
    /// <param name="password">The password with special characters to test.</param>
    [Theory]
    [InlineData("P@ssw0rd")]
    [InlineData("MyP@ss123!")]
    [InlineData("Complex#Pass1")]
    public void Constructor_PasswordWithSpecialCharacters_PassesValidation(string password)
    {
        // Arrange
        var validator = new RegisterWithPasswordCommandValidator();
        var command = new RegisterWithPasswordCommand("user@example.com", "John", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}