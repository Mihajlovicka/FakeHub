using FakeHubApi.Filters;
using FakeHubApi.Mapper;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Repository.Implementation;

namespace FakeHubApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Scoped services registration
        services.AddScoped<ValidationFilterAttribute>();

        // Mapper-related scoped services
        services.AddScoped<IMapperManager, MapperManager>();

        // Repository-related scoped services
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}
