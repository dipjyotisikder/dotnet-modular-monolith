namespace Users.Features.UnitTests.UserManagement.CreateUser;

/// <summary>
/// Unit tests for <see cref="CreateUserCommandValidator"/>.
/// </summary>
public class CreateUserCommandValidatorTests
{
    /// <summary>
    /// Tests that the validator passes for a command with valid email and valid name.
    /// </summary>
    /// <param name="email">The valid email address.</param>
    /// <param name="name">The valid name (2+ characters).</param>
    [Theory]
    [InlineData("user@example.com", "John")]
    [InlineData("test.user@domain.com", "Jane Doe")]
    [InlineData("valid@email.org", "AB")]
    [InlineData("another.email@test.co.uk", "Very Long Name With Multiple Words")]
    public void Constructor_ValidEmailAndName_ValidationPasses(string email, string name)
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(email, name);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that the validator fails with the correct error message when the email is empty or invalid.
    /// </summary>
    /// <param name="email">The invalid email value.</param>
    /// <param name="expectedErrorMessage">The expected validation error message.</param>
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
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(email, "ValidName");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// Tests that the validator fails with the correct error message when the name is empty or too short.
    /// </summary>
    /// <param name="name">The invalid name value.</param>
    /// <param name="expectedErrorMessage">The expected validation error message.</param>
    [Theory]
    [InlineData("", "Name is required")]
    [InlineData(" ", "Name is required")]
    [InlineData("   ", "Name is required")]
    [InlineData("A", "Name must be at least 2 characters")]
    public void Constructor_InvalidName_ValidationFailsWithCorrectMessage(string name, string expectedErrorMessage)
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("valid@email.com", name);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(expectedErrorMessage);
    }

    /// <summary>
    /// Tests that the validator fails with multiple errors when both email and name are invalid.
    /// </summary>
    [Fact]
    public void Constructor_InvalidEmailAndName_ValidationFailsWithMultipleErrors()
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("", "");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    /// <summary>
    /// Tests that the validator passes when the name is exactly 2 characters (boundary condition).
    /// </summary>
    [Fact]
    public void Constructor_NameWithExactlyTwoCharacters_ValidationPasses()
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("valid@email.com", "AB");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that the validator fails when the email contains special characters that make it invalid.
    /// </summary>
    /// <param name="email">The email with special characters.</param>
    [Theory]
    [InlineData("user name@example.com")]
    [InlineData("user\t@example.com")]
    [InlineData("user\n@example.com")]
    [InlineData("user,name@example.com")]
    public void Constructor_EmailWithInvalidSpecialCharacters_ValidationFails(string email)
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(email, "ValidName");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be valid");
    }

    /// <summary>
    /// Tests that the validator passes for names with special but valid characters.
    /// </summary>
    /// <param name="name">The name with special characters.</param>
    [Theory]
    [InlineData("O'Brien")]
    [InlineData("Mary-Jane")]
    [InlineData("José")]
    [InlineData("François")]
    [InlineData("名前")]
    public void Constructor_NameWithValidSpecialCharacters_ValidationPasses(string name)
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand("valid@email.com", name);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that the validator handles very long valid inputs correctly.
    /// </summary>
    [Fact]
    public void Constructor_VeryLongValidInputs_ValidationPasses()
    {
        // Arrange
        var validator = new CreateUserCommandValidator();
        var longEmail = new string('a', 50) + "@" + new string('b', 50) + ".com";
        var longName = new string('N', 1000);
        var command = new CreateUserCommand(longEmail, longName);

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}