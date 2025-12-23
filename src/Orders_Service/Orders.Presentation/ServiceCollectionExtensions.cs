using Microsoft.Extensions.DependencyInjection;

namespace Orders.Presentation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}