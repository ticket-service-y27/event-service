using EventService.Application.Abstractions.Messaging;
using EventService.Application.Models.Events;
using EventService.Infrastructure.Messaging.Contracts;
using EventService.Presentation.Kafka.Abstractions;
using EventService.Presentation.Kafka.Options;

namespace EventService.Presentation.Kafka.Publishers;

public sealed class EventCreatedPublisher : IEventCreatedPublisher
{
    private readonly IKafkaProducer<long, EventCreatedValue> _producer;
    private readonly KafkaOptions _options;

    public EventCreatedPublisher(IKafkaProducer<long, EventCreatedValue> producer, KafkaOptions options)
    {
        _producer = producer;
        _options = options;
    }

    public Task PublishAsync(EventCreatedEvent evt, CancellationToken ct)
    {
        if (_options.EventCreatedTopic == null)
            throw new ArgumentNullException(_options.EventCreatedTopic);

        var value = new EventCreatedValue
        {
            EventId = evt.EventId,
            ArtistId = evt.ArtistId,
            TotalSeats = evt.TotalSeats,
            EventDate = evt.EventDate.ToUnixTimeSeconds(),
        };

        return _producer.ProduceAsync(_options.EventCreatedTopic, evt.EventId, value, ct);
    }
}