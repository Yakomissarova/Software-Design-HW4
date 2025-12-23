using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Repositories;
using Orders.UseCases.Abstractions;

namespace Orders.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("OrdersDb") ?? "Data Source=orders.db";
        services.AddDbContext<OrdersDbContext>(o => o.UseSqlite(cs));

        var opt = new RabbitMqOptions();
        config.GetSection("RabbitMq").Bind(opt);
        services.AddSingleton(opt);

        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<RabbitMqPublisher>();

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<OutboxDispatcher>();
        services.AddHostedService<PaymentResultConsumer>();

        return services;
    }
}