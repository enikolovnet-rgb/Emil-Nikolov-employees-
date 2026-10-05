using EmployeePairs.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ServicesDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IEmployeePairService, EmployeePairService>();

        return services;
    }
}
