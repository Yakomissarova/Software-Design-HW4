namespace Payments.Infrastructure.Messaging;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "rabbitmq";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string PaymentRequestsQueue { get; set; } = "orders.payment.requested";
    public string PaymentResultsQueue { get; set; } = "orders.payment.result";
}