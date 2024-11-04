using AuthService.Mapper.UserMapper;
using FakeHubApi.Filters;
using FakeHubApi.Mapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Repository.Implementation;
using FakeHubApi.Service.Contract;

namespace FakeHubApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Scoped services registration
        services.AddScoped<IAuthService, Service.Implementation.AuthService>();
        services.AddScoped<ValidationFilterAttribute>();

        // Mapper-related scoped services
        services.AddScoped<IBaseMapper<RegistrationRequestDto, ApplicationUser>, RegistrationsRequestDtoToApplicationUser>();
        services.AddScoped<IMapperManager, MapperManager>();

        // Repository-related scoped services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}
