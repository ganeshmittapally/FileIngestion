using System.Threading;
using System.Threading.Tasks;

namespace FileIngestion.Application.Ports;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);
}
