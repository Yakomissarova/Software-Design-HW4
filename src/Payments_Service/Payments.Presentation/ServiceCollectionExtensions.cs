using Microsoft.Extensions.DependencyInjection;

namespace Payments.Presentation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}