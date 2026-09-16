using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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

        return services;
    }
}
