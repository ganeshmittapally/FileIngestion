using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FileIngestion.Application.Ports;

namespace FileIngestion.Infrastructure.Adapters;

// This file is included in builds only when UseServiceBus=true
// and the Azure.Messaging.ServiceBus package is available.
public class ServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _client;
    private readonly string _queueOrTopic;

    public ServiceBusEventPublisher(ServiceBusClient client, string queueOrTopic)
    {
        _client = client;
        _queueOrTopic = queueOrTopic;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        var sender = _client.CreateSender(_queueOrTopic);
        var json = JsonSerializer.Serialize(message);
        using var sbMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };
        await sender.SendMessageAsync(sbMessage, cancellationToken).ConfigureAwait(false);
    }
}
