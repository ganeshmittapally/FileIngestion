using System.Threading;
using System.Threading.Tasks;
using FileIngestion.Application.Ports;

namespace FileIngestion.Infrastructure.Adapters;

// Default no-op event publisher used in local/dev builds
public class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        // intentionally noop
        return Task.CompletedTask;
    }
}
