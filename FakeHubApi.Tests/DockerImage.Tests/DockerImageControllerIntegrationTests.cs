using System.Net.Http.Json;
using FakeHubApi.Model.ServiceResponse;

namespace FakeHubApi.Tests.DockerImage.Tests
{
    public class DockerImageControllerIntegrationTests
    {
        private HttpClient _client;
        private CustomWebApplicationFactory _factory;

        [OneTimeSetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task GetDockerImages_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/docker-images");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();

            Assert.That(responseObj?.Success, Is.True);
        }
    }
}
