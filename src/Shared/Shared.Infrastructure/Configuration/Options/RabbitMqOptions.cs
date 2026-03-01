namespace Shared.Infrastructure.Configuration.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public bool Enabled { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 5672;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? VirtualHost { get; set; }
    public string? Exchange { get; set; }
}
