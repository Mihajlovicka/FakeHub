using FakeHubApi.Filters;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Mvc;

namespace FakeHubApi.Controllers
{
    [Route("api/docker-images")]
    [ApiController]
    public class DockerImageController(IDockerImageService dockerImageService) : ControllerBase
    {
        private readonly IDockerImageService _dockerImageService = dockerImageService;

        [HttpGet]
        public async Task<IActionResult> GetDockerImages()
        {
            var dockerImages = await _dockerImageService.GetDockerImagesAsync();

            return Ok(dockerImages);
        }
    }
}
