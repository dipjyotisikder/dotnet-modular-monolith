namespace Users.Features.UnitTests.Authentication.Login;

/// <summary>
/// Unit tests for <see cref="LoginCommandValidator"/>.
/// </summary>
public class LoginCommandValidatorTests
{
    /// <summary>
    /// Tests that the validator can be successfully instantiated.
    /// </summary>
    [Fact]
    public void Constructor_WhenCalled_CreatesValidatorInstance()
    {
        // Arrange & Act
        var validator = new LoginCommandValidator();

        // Assert
        Assert.NotNull(validator);
    }

    /// <summary>
    /// Tests that validation passes when both Email and Password are valid.
    /// </summary>
    /// <param name="email">Valid email address.</param>
    /// <param name="password">Valid password (6+ characters).</param>
    [Theory]
    [InlineData("test@example.com", "password123")]
    [InlineData("user.name@domain.co.uk", "123456")]
    [InlineData("valid+email@test.com", "verylongpassword")]
    [InlineData("a@b.c", "minimumpass")]
    public void Constructor_ValidEmailAndPassword_ValidationPasses(string email, string password)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that validation fails when Email is null with the expected error message.
    /// </summary>
    [Fact]
    public void Constructor_NullEmail_ValidationFailsWithExpectedMessage()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(null!, "validpassword");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    /// <summary>
    /// Tests that validation fails when Email is empty with the expected error message.
    /// </summary>
    [Fact]
    public void Constructor_EmptyEmail_ValidationFailsWithExpectedMessage()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(string.Empty, "validpassword");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    /// <summary>
    /// Tests that validation fails when Email is whitespace-only.
    /// </summary>
    /// <param name="email">Whitespace-only email value.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_WhitespaceEmail_ValidationFails(string email)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, "validpassword");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Tests that validation fails when Email is not a valid email address with the expected error message.
    /// </summary>
    /// <param name="invalidEmail">Invalid email address format.</param>
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
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(invalidEmail, "validpassword");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }

    /// <summary>
    /// Tests that validation fails when Password is null with the expected error message.
    /// </summary>
    [Fact]
    public void Constructor_NullPassword_ValidationFailsWithExpectedMessage()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", null!);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    /// <summary>
    /// Tests that validation fails when Password is empty with the expected error message.
    /// </summary>
    [Fact]
    public void Constructor_EmptyPassword_ValidationFailsWithExpectedMessage()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", string.Empty);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    /// <summary>
    /// Tests that validation fails when Password is whitespace-only.
    /// </summary>
    /// <param name="password">Whitespace-only password value.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_WhitespacePassword_ValidationFails(string password)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation fails when Password is less than 6 characters with the expected error message.
    /// </summary>
    /// <param name="shortPassword">Password with less than 6 characters.</param>
    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("12345")]
    public void Constructor_PasswordLessThanSixCharacters_ValidationFailsWithExpectedMessage(string shortPassword)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", shortPassword);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 6 characters");
    }

    /// <summary>
    /// Tests that validation passes when Password is exactly 6 characters (boundary condition).
    /// </summary>
    [Fact]
    public void Constructor_PasswordExactlySixCharacters_ValidationPasses()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", "123456");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation passes when Password is more than 6 characters.
    /// </summary>
    /// <param name="password">Password with more than 6 characters.</param>
    [Theory]
    [InlineData("1234567")]
    [InlineData("averylongpassword")]
    [InlineData("P@ssw0rd!")]
    public void Constructor_PasswordMoreThanSixCharacters_ValidationPasses(string password)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation fails with multiple errors when both Email and Password are invalid.
    /// </summary>
    [Fact]
    public void Constructor_BothEmailAndPasswordInvalid_ValidationFailsWithMultipleErrors()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(string.Empty, "123");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation handles very long valid Email addresses.
    /// </summary>
    [Fact]
    public void Constructor_VeryLongValidEmail_ValidationPasses()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var longEmail = new string('a', 50) + "@" + new string('b', 50) + ".com";
        var command = new LoginCommand(longEmail, "validpassword");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Tests that validation handles very long valid Password.
    /// </summary>
    [Fact]
    public void Constructor_VeryLongPassword_ValidationPasses()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var longPassword = new string('x', 1000);
        var command = new LoginCommand("valid@email.com", longPassword);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Tests that validation handles Email with special characters (valid format).
    /// </summary>
    /// <param name="email">Email with special characters.</param>
    [Theory]
    [InlineData("user+tag@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user_name@example.com")]
    [InlineData("123@example.com")]
    public void Constructor_EmailWithSpecialCharacters_ValidationPasses(string email)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, "validpassword");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Tests that validation handles Password with special characters.
    /// </summary>
    /// <param name="password">Password with special characters.</param>
    [Theory]
    [InlineData("P@ssw0rd!")]
    [InlineData("p@$$w0rd")]
    [InlineData("pass word")]
    [InlineData("pass\tword")]
    public void Constructor_PasswordWithSpecialCharacters_ValidationPasses(string password)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("valid@email.com", password);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}