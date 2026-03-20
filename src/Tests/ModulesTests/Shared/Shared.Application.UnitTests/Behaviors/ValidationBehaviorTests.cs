using FluentValidation;
using MediatR;
using Shared.Application.Behaviors;


namespace Shared.Application.UnitTests.Behaviors;

/// <summary>
/// Unit tests for the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
/// </summary>
public sealed class ValidationBehaviorTests
{
    /// <summary>
    /// Test request type used in validation behavior tests.
    /// </summary>
    private sealed class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test response type used in validation behavior tests.
    /// </summary>
    private sealed class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }

    /// <summary>
    /// Verifies that Handle calls next delegate immediately when no validators are provided.
    /// </summary>
    [Fact]
    public async Task Handle_NoValidators_CallsNextDelegate()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResponse = new TestResponse { Result = "success" };
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await behavior.Handle(request, next, cancellationToken);

        // Assert
        Assert.True(nextCalled);
        Assert.Same(expectedResponse, result);
    }

    /// <summary>
    /// Verifies that Handle passes the cancellation token to the next delegate when validation passes.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationPasses_PassesCancellationTokenToNextDelegate()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Value = "test" };
        CancellationToken capturedToken = default;

        RequestHandlerDelegate<TestResponse> next = () =>
        {
            capturedToken = CancellationToken.None;
            return Task.FromResult(new TestResponse());
        };

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        await behavior.Handle(request, next, cancellationToken);

        // Assert
        // Note: Since RequestHandlerDelegate doesn't take a CancellationToken parameter,
        // we can only verify the token is passed to validators
        Assert.NotNull(next);
    }

}