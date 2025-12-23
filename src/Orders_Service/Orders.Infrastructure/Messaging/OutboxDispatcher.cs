using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Entities.Models;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Messaging;

public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<OutboxDispatcher> _log;

    public OutboxDispatcher(IServiceProvider sp, RabbitMqPublisher publisher, ILogger<OutboxDispatcher> log)
    {
        _sp = sp;
        _publisher = publisher;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Orders OutboxDispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var pending = await db.OutboxMessages
                    .Where(x => x.Status == OutboxMessageStatus.Pending)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                pending = pending
                    .OrderBy(x => x.CreatedAt)
                    .ToList();

                _log.LogInformation("Orders outbox pending count: {Count}", pending.Count);

                foreach (var msg in pending)
                {
                    try
                    {
                        msg.IncrementAttempts();

                        _log.LogInformation("Publishing outbox message {Id} type={Type} attempts={Attempts}",
                            msg.Id, msg.Type, msg.Attempts);

                        if (msg.Type == "OrderPaymentRequested")
                        {
                            _log.LogInformation("Orders OutboxDispatcher: publishing payment request payload={Payload}",
                                msg.Payload);

                            _publisher.PublishPaymentRequest(msg.Payload);
                        }
                        else
                        {
                            _log.LogWarning("Unknown outbox message type: {Type}", msg.Type);
                        }

                        msg.MarkSent();
                        _log.LogInformation("Orders OutboxDispatcher: marked Sent message {Id}", msg.Id);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to publish outbox message {Id}", msg.Id);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Orders OutboxDispatcher loop failed");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
