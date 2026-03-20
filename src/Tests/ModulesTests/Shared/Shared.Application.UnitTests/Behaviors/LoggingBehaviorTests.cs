using Shared.Application.Behaviors;

namespace Shared.Application.UnitTests.Behaviors;

/// <summary>
/// Unit tests for the <see cref="LoggingBehavior{TRequest, TResponse}"/> class.
/// </summary>
public sealed class LoggingBehaviorTests
{
    /// <summary>
    /// Helper class representing a test request.
    /// </summary>
    private sealed class TestRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// Helper class representing an alternative test request.
    /// </summary>
    private sealed class AlternativeTestRequest
    {
        public int Number { get; set; }
    }

    /// <summary>
    /// Helper class representing a test response.
    /// </summary>
    private sealed class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }
}