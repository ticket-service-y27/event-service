using EventService.Application.Abstractions.Messaging;
using EventService.Application.Models.Events;
using EventService.Infrastructure.Messaging.Contracts;
using EventService.Presentation.Kafka.Abstractions;
using EventService.Presentation.Kafka.Options;

namespace EventService.Presentation.Kafka.Publishers;

public sealed class SeatReturnedPublisher : ISeatReturnedPublisher
{
    private readonly IKafkaProducer<long, SeatsReturnedValue> _producer;
    private readonly KafkaOptions _options;

    public SeatReturnedPublisher(IKafkaProducer<long, SeatsReturnedValue> producer, KafkaOptions options)
    {
        _producer = producer;
        _options = options;
    }

    public Task PublishAsync(SeatReturnedEvent evt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.SeatReturnedTopic))
            throw new ArgumentNullException(_options.SeatReturnedTopic);

        var value = new SeatsReturnedValue
        {
            HallSchemeId = evt.HallSchemeId,
            ReturnedSeats = evt.ReturnedSeats,
        };

        return _producer.ProduceAsync(_options.SeatReturnedTopic, evt.HallSchemeId, value, ct);
    }
}