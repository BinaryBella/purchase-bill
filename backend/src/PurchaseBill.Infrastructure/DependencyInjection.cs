using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PurchaseBill.Application.Interfaces;
using PurchaseBill.Infrastructure.Auth;
using PurchaseBill.Infrastructure.ExternalServices;
using PurchaseBill.Infrastructure.Persistence;

namespace PurchaseBill.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.Configure<EnhanzerOptions>(configuration.GetSection(EnhanzerOptions.SectionName));
        services.AddHttpClient<IEnhanzerAuthClient, EnhanzerAuthClient>((sp, client) =>
        {
            var baseUrl = configuration[$"{EnhanzerOptions.SectionName}:BaseUrl"]
                ?? throw new InvalidOperationException("Enhanzer:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
