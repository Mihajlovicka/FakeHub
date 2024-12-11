using FakeHubApi.Filters;
using FakeHubApi.Mapper;
using FakeHubApi.Mapper.OrganizationMapper;
using FakeHubApi.Mapper.TeamMapper;
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
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ITeamService, TeamService>();

        // Mapper-related scoped services
        services.AddScoped<
            IBaseMapper<RegistrationRequestDto, User>,
            RegistrationsRequestDtoToApplicationUser
        >();
        services.AddScoped<
            IBaseMapper<User, UserProfileResponseDto>,
            ApplicationUserToUserProfileResponseDto
        >();
        services.AddScoped<
            IBaseMapper<OrganizationDto, Organization>,
            OrganizationDtoToOgranization
        >();
        services.AddScoped<IBaseMapper<TeamDto, Team>, TeamDtoToTeam>();
        services.AddScoped<IMapperManager, MapperManager>();

        // Repository-related scoped services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}
