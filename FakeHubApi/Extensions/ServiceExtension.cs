using FakeHubApi.ContainerRegistry;
using FakeHubApi.ElasticSearch;
using FakeHubApi.Filters;
using FakeHubApi.Mapper;
using FakeHubApi.Mapper.OrganizationMapper;
using FakeHubApi.Mapper.RepositoryMapper;
using FakeHubApi.Mapper.TeamMapper;
using FakeHubApi.Mapper.UserMapper;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.Settings;
using FakeHubApi.Redis;
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

        services.AddHttpClient("FakeHubApi")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseCookies = false,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; },
                AllowAutoRedirect = false, 
            });

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<HarborSettings>(configuration.GetSection("Harbor"));


        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ValidationFilter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IRepositoryService, RepositoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        
        services.AddSingleton<IHarborTokenService, HarborTokenService>();
        services.AddSingleton<IHarborService, HarborService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "FakeHub_";
        });

        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<ElasticSearchService>();

        // Mapper-related scoped services
        services.AddScoped<
            IBaseMapper<RegistrationRequestDto, User>,
            RegistrationsRequestDtoToUser
        >();
        services.AddScoped<
            IBaseMapper<User, UserDto>,
            UserToUserDto
        >();
        services.AddScoped<IBaseMapper<TeamDto, Team>, TeamDtoToTeam>();
        services.AddScoped<
            IBaseMapper<OrganizationDto, Organization>,
            OrganizationDtoToOrganization
        >();
        services.AddScoped<IMapperManager, MapperManager>();
        services.AddScoped<IBaseMapper<RepositoryDto, Model.Entity.Repository>, RepositoryDtoToRepositoryMapper>();

        // Repository-related scoped services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IRepositoryRepository, RepositoryRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        
        return services;
    }
}
