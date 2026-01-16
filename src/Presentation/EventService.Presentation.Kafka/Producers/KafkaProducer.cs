using Confluent.Kafka;
using EventService.Presentation.Kafka.Abstractions;

namespace EventService.Presentation.Kafka.Producers;

public class KafkaProducer<TKey, TValue> : IKafkaProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;

    public KafkaProducer(IProducer<TKey, TValue> producer)
    {
        _producer = producer;
    }

    public async Task ProduceAsync(string topic, TKey key, TValue value, CancellationToken ct = default)
    {
        await _producer.ProduceAsync(
            topic,
            new Message<TKey, TValue>
            {
                Key = key,
                Value = value,
            });
    }
}