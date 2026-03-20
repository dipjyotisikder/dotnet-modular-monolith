namespace Shared.Application.UnitTests.Behaviors;

public sealed class LoggingBehaviorTests
{
    private sealed class TestRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class AlternativeTestRequest
    {
        public int Number { get; set; }
    }

    private sealed class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }
}
