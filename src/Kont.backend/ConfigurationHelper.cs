using Kont.backend.Models;

namespace Kont.backend;

public static class ConfigurationHelper
{
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AppSettings>(config.GetSection($"AppSettings"));

        return services;
    }
}
