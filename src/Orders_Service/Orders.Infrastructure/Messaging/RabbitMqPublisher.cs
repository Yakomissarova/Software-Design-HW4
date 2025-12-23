using System.Text;
using RabbitMQ.Client;

namespace Orders.Infrastructure.Messaging;

public class RabbitMqPublisher
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly RabbitMqOptions _opt;

    public RabbitMqPublisher(RabbitMqConnectionFactory factory, RabbitMqOptions opt)
    {
        _factory = factory;
        _opt = opt;
    }

    public void PublishPaymentRequest(string messageBody)
    {
        using var channel = _factory.GetConnection().CreateModel();

        channel.QueueDeclare(_opt.PaymentRequestsQueue, durable: true, exclusive: false, autoDelete: false);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        var bytes = Encoding.UTF8.GetBytes(messageBody);
        channel.BasicPublish("", _opt.PaymentRequestsQueue, props, bytes);
    }
}