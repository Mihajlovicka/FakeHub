using FakeHubApi.Filters;
using FakeHubApi.Mapper;
using FakeHubApi.Mapper.UserMapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.Settings;
using FakeHubApi.Repository.Contract;
using FakeHubApi.Repository.Implementation;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;

namespace FakeHubApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Service-related scoped services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDockerImageService, DockerImageService>();
        services.AddScoped<ValidationFilter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Mapper-related scoped services
        services.AddScoped<
            IBaseMapper<RegistrationRequestDto, User>,
            RegistrationsRequestDtoToApplicationUser
        >();
        services.AddScoped<IMapperManager, MapperManager>();

        // Repository-related scoped services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}