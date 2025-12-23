using Microsoft.Extensions.DependencyInjection;
using Payments.UseCases.Commands.CreateAccount;
using Payments.UseCases.Commands.TopUpAccount;
using Payments.UseCases.Commands.ProcessPayment;
using Payments.UseCases.Queries.GetBalance;

namespace Payments.UseCases;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPaymentsUseCases(this IServiceCollection services)
    {
        services.AddScoped<CreateAccountHandler>();
        services.AddScoped<TopUpAccountHandler>();
        services.AddScoped<ProcessPaymentHandler>();
        services.AddScoped<GetBalanceHandler>();

        return services;
    }
}