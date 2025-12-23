using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Payments.UseCases.Commands.ProcessPayment;

namespace Payments.Infrastructure.Messaging;

public class PaymentRequestConsumer : BackgroundService
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly RabbitMqOptions _opt;
    private readonly IServiceProvider _sp;
    private readonly ILogger<PaymentRequestConsumer> _log;

    private static readonly JsonSerializerOptions JsonOpt = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public PaymentRequestConsumer(
        RabbitMqConnectionFactory factory,
        RabbitMqOptions opt,
        IServiceProvider sp,
        ILogger<PaymentRequestConsumer> log)
    {
        _factory = factory;
        _opt = opt;
        _sp = sp;
        _log = log;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Payments PaymentRequestConsumer started, queue={Queue}", _opt.PaymentRequestsQueue);

        var channel = _factory.GetConnection().CreateModel();
        channel.QueueDeclare(_opt.PaymentRequestsQueue, durable: true, exclusive: false, autoDelete: false);
        channel.BasicQos(0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            _log.LogInformation("Payments received payment request: {Json}", json);

            try
            {
                var dto = JsonSerializer.Deserialize<PaymentRequestDto>(json, JsonOpt)
                          ?? throw new InvalidOperationException("Invalid message (null)");

                using var scope = _sp.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ProcessPaymentHandler>();

                // 4-й параметр: correlation/message id (если в твоей команде 4-й другой — скажи, я подставлю правильно)
                var messageId = dto.MessageId ?? Guid.NewGuid();

                await handler.Handle(
                    new ProcessPaymentCommand(messageId, dto.OrderId, dto.UserId, dto.Amount),
                    stoppingToken);


                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Payments failed to process message");

                var requeue =
                    ex is TimeoutException
                    || ex is IOException; // только реально временные

                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: requeue);
            }

        };

        channel.BasicConsume(_opt.PaymentRequestsQueue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    // DTO должен совпадать с тем, что Orders кладёт в outbox payload
    private record PaymentRequestDto(
        Guid OrderId,
        Guid UserId,
        decimal Amount,
        string? Login,
        Guid? MessageId
    );
}
