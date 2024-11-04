namespace FakeHubApi.Extensions;

public static class CorsExtensions
{
    private const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        return services;
    }

    public static string GetCorsPolicyName() => MyAllowSpecificOrigins;
}