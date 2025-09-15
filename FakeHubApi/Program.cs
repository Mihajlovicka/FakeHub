using System.Net;
using FakeHubApi.Data;
using FakeHubApi.Extensions;
using FakeHubApi.Filters;
using FakeHubApi.Helpers;
using FakeHubApi.Model.Entity;
using FakeHubApi.Service.Contract;
using FakeHubApi.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == "Docker")
{
    builder.Configuration.AddJsonFile("appsettings.Docker.json", optional: true, reloadOnChange: true);
}
builder.Configuration.AddEnvironmentVariables();

// builder.Host.UseSerilog((context, configuration) =>
//     configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<AppDbContext>(option =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is not found.");
    }
    option.UseMySQL(connectionString);
});

// Register custom services, CORS, and Identity
builder.Services.AddCustomCors();
builder.Services.AddCustomServices(builder.Configuration);

builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.SuppressModelStateInvalidFilter = true
);

builder
    .Services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.AddAuthenticationAndAuthorization();

builder.Services.AddScoped<IUserContextService, UserContextService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer",
        }
    );

    opt.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminPolicy", policy => policy.RequireRole("ADMIN"))
    .AddPolicy("UserPolicy", policy => policy.RequireRole("USER"))
    .AddPolicy("SuperAdminPolicy", policy => policy.RequireRole("SUPERADMIN"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NoRolePolicy", policy =>
        policy.Requirements.Add(new NoRoleRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, NoRoleHandler>();

var app = builder.Build();

// Seed the superadmin user and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await AppDbContextSeed.SeedSuperAdminUserAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.GetCorsPolicyName());
app.UseAuthentication();
app.UseAuthorization();

// app.UseSerilogRequestLogging();

app.MapControllers();
app.ApplyPendingMigrations();

app.Run();
