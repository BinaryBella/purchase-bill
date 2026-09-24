using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PurchaseBill.Application.Services;

namespace PurchaseBill.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IPurchaseBillService, PurchaseBillService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
