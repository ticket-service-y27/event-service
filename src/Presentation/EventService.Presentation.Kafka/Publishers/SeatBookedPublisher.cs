using EventService.Application.Abstractions.Messaging;
using EventService.Application.Models.Events;
using EventService.Infrastructure.Messaging.Contracts;
using EventService.Presentation.Kafka.Abstractions;
using EventService.Presentation.Kafka.Options;

namespace EventService.Presentation.Kafka.Publishers;

public sealed class SeatBookedPublisher : ISeatBookedPublisher
{
    private readonly IKafkaProducer<long, SeatsBookedValue> _producer;
    private readonly KafkaOptions _options;

    public SeatBookedPublisher(IKafkaProducer<long, SeatsBookedValue> producer, KafkaOptions options)
    {
        _producer = producer;
        _options = options;
    }

    public Task PublishAsync(SeatBookedEvent evt, CancellationToken ct)
    {
        if (_options.SeatBookedTopic == null)
            throw new ArgumentNullException(_options.SeatBookedTopic);

        var value = new SeatsBookedValue
        {
            EventId = evt.EventId,
            BookedSeats = evt.BookedSeats,
        };

        return _producer.ProduceAsync(_options.SeatBookedTopic, evt.EventId, value, ct);
    }
}