using Microsoft.Extensions.DependencyInjection;
using Orders.UseCases.Commands.CreateOrder;
using Orders.UseCases.Commands.ProcessPaymentResult;
using Orders.UseCases.Queries.GetOrders;
using Orders.UseCases.Queries.GetOrderById;

namespace Orders.UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<ProcessPaymentResultHandler>();
        services.AddScoped<GetOrdersHandler>();
        services.AddScoped<GetOrderByIdHandler>();

        return services;
    }
}