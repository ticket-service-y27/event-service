using EventService.Application.Models.Events;
using EventService.Infrastructure.Messaging.Contracts;
using EventService.Presentation.Kafka.Abstractions;
using EventService.Presentation.Kafka.Options;

namespace EventService.Presentation.Kafka.Publishers;

public sealed class VenueCreatedPublisher : IVenueCreatedPublisher
{
    private readonly IKafkaProducer<long, VenueCreatedValue> _producer;
    private readonly KafkaOptions _options;

    public VenueCreatedPublisher(IKafkaProducer<long, VenueCreatedValue> producer, KafkaOptions options)
    {
        _producer = producer;
        _options = options;
    }

    public Task PublishAsync(VenueCreatedEvent evt, CancellationToken ct)
    {
        if (_options.VenueCreatedTopic == null)
            throw new ArgumentNullException(_options.VenueCreatedTopic);

        var value = new VenueCreatedValue
        {
            VenueId = evt.VenueId,
            TotalSeats = evt.TotalSeats,
            Address = evt.Address,
        };

        return _producer.ProduceAsync(_options.VenueCreatedTopic, evt.VenueId, value, ct);
    }
}