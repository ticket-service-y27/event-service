namespace EventService.Presentation.Kafka.Options;

public class KafkaOptions
{
    public string BootstrapServers { get; init; } = string.Empty;

    public string EventCreatedTopic { get; init; } = string.Empty;

    public string SeatBookedTopic { get; init; } = string.Empty;

    public string VenueCreatedTopic { get; init; } = string.Empty;

    public int BatchSize { get; init; } = 100;

    public int PollTimeoutMs { get; init; } = 500;
}