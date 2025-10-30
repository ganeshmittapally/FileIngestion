using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileIngestion.Infrastructure;

// Included only when UseServiceBus=true (and Azure.Messaging.ServiceBus available)
public static class ServiceBusRegistration
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration["Azure:ServiceBus:ConnectionString"];
        var queue = configuration["Azure:ServiceBus:QueueName"];
        if (string.IsNullOrEmpty(conn) || string.IsNullOrEmpty(queue)) return;

        services.AddSingleton(sp => new ServiceBusClient(conn));
        services.AddSingleton<FileIngestion.Application.Ports.IEventPublisher>(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return new Adapters.ServiceBusEventPublisher(client, queue);
        });
    }
}
