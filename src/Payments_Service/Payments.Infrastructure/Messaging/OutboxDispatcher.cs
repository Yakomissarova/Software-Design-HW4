using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payments.Entities.Models;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Messaging;

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
        _log.LogInformation("Payments OutboxDispatcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                // IMPORTANT:
                // SQLite не умеет ORDER BY по DateTimeOffset => сортируем уже в памяти
                var pending = await db.OutboxMessages
                    .Where(x => x.Status == OutboxMessageStatus.Pending)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                pending = pending
                    .OrderBy(x => x.CreatedAt)
                    .ToList();

                _log.LogInformation("Payments outbox pending count: {Count}", pending.Count);

                foreach (var msg in pending)
                {
                    try
                    {
                        msg.IncrementAttempts();

                        _log.LogInformation("Publishing outbox message {Id} type={Type}", msg.Id, msg.Type);

                        if (msg.Type == "OrderPaymentResult")
                            _publisher.PublishPaymentResult(msg.Payload);
                        else
                            _log.LogWarning("Unknown outbox message type: {Type}", msg.Type);

                        msg.MarkSent();
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to publish outbox message {Id}", msg.Id);
                        // Не помечаем как Sent => попробуем ещё раз
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Payments OutboxDispatcher loop failed");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
