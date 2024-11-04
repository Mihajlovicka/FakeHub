using System.Net.Http.Json;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Tests;

public class AuthControllerIntegrationTests
{
    private HttpClient _client;
    private CustomWebApplicationFactory _factory;

    [SetUp]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Register_UserCreatedSuccessfully_ReturnsOk()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "test@example.com",
            Username = "UserName",
            Password = "Password123!",
            Role = "USER"
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequestDto);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.IsTrue(responseObj?.Success);
    }

    [Test]
    public async Task Register_ErrorReturned_VaidationError()
    {
        // Arrange
        var registrationRequestDto = new RegistrationRequestDto
        {
            Email = "testexample",
            Username = "UserName",
            Password = "Password123!",
            Role = "USER"
        };
        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registrationRequestDto);
        
        // Assert
        var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
        Assert.IsFalse(responseObj?.Success);
    }
}
