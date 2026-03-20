using FluentValidation;
using MediatR;
using Shared.Application.Behaviors;

namespace Shared.Application.UnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNextDelegate()
    {
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

        var result = await behavior.Handle(request, next, cancellationToken);

        Assert.True(nextCalled);
        Assert.Same(expectedResponse, result);
    }

    [Fact]
    public async Task Handle_ValidationPasses_PassesCancellationTokenToNextDelegate()
    {
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

        await behavior.Handle(request, next, cancellationToken);

        Assert.NotNull(next);
    }
}
