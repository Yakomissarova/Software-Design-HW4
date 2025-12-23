using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Orders.UseCases.Commands.ProcessPaymentResult;

namespace Orders.Infrastructure.Messaging;

public class PaymentResultConsumer : BackgroundService
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly RabbitMqOptions _opt;
    private readonly IServiceProvider _sp;
    private readonly ILogger<PaymentResultConsumer> _log;

    private static readonly JsonSerializerOptions JsonOpt = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaymentResultConsumer(
        RabbitMqConnectionFactory factory,
        RabbitMqOptions opt,
        IServiceProvider sp,
        ILogger<PaymentResultConsumer> log)
    {
        _factory = factory;
        _opt = opt;
        _sp = sp;
        _log = log;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Orders PaymentResultConsumer started, queue={Queue}", _opt.PaymentResultsQueue);

        var channel = _factory.GetConnection().CreateModel();
        channel.QueueDeclare(_opt.PaymentResultsQueue, durable: true, exclusive: false, autoDelete: false);
        channel.BasicQos(0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            _log.LogInformation("Orders received payment result: {Json}", json);

            try
            {
                var dto = JsonSerializer.Deserialize<PaymentResultDto>(json, JsonOpt)
                          ?? throw new InvalidOperationException("Invalid message");

                using var scope = _sp.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ProcessPaymentResultHandler>();

                _log.LogInformation("Orders PaymentResultConsumer: dispatching to handler OrderId={OrderId} Status={Status}",
                    dto.OrderId, dto.Status);

                await handler.Handle(new ProcessPaymentResultCommand(dto.OrderId, dto.Status), stoppingToken);

                channel.BasicAck(ea.DeliveryTag, false);
                _log.LogInformation("Orders PaymentResultConsumer: ACK deliveryTag={Tag}", ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Orders PaymentResultConsumer: failed, will requeue");
                channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        channel.BasicConsume(_opt.PaymentResultsQueue, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    private record PaymentResultDto(Guid OrderId, string Status);
}
