using FakeHubApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySql.Data.MySqlClient;

namespace FakeHubApi.Tests;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database") ?? "Server=localhost;Database=fake-hub;Uid=root;Pwd=admin;";
                                    // ?? "Server=localhost;Port=3306;Database=fake-hub-test;Uid=root;Pwd=admin;";

        _databaseName = $"fake_hub_test_{Guid.NewGuid()}";
        
        var builder = new MySqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _databaseName
        };
        _connectionString = builder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));

            var baseConnectionString = new MySqlConnectionStringBuilder(_connectionString) { Database = "" }.ConnectionString;

            using var connection = new MySqlConnection(baseConnectionString);
            connection.Open();

            using var command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{_databaseName}`;", connection);
            command.ExecuteNonQuery();

            services.AddDbContext<AppDbContext>(options => options.UseMySQL(_connectionString));

            var dbContext = CreateDbContext(services);
            dbContext.Database.EnsureDeleted();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var baseConnectionString = new MySqlConnectionStringBuilder(_connectionString) { Database = "" }.ConnectionString;

            using var connection = new MySqlConnection(baseConnectionString);
            connection.Open();

            using var command = new MySqlCommand($"DROP DATABASE IF EXISTS `{_databaseName}`;", connection);
            command.ExecuteNonQuery();
        }

        base.Dispose(disposing);
    }

    private static AppDbContext CreateDbContext(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
