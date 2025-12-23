using RabbitMQ.Client;

namespace Payments.Infrastructure.Messaging;

public class RabbitMqConnectionFactory
{
    private readonly RabbitMqOptions _opt;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMqConnectionFactory(RabbitMqOptions opt) => _opt = opt;

    public IConnection GetConnection()
    {
        lock (_lock)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _opt.HostName,
                UserName = _opt.UserName,
                Password = _opt.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            // retry loop (простая учебная версия)
            var delay = TimeSpan.FromSeconds(1);
            for (var attempt = 1; attempt <= 30; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    return _connection;
                }
                catch
                {
                    Thread.Sleep(delay);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 5));
                }
            }

            // если совсем не получилось
            _connection = factory.CreateConnection();
            return _connection;
        }
    }

}