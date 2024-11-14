using FakeHubApi.Model.Dto;

namespace FakeHubApi.Mocks
{
    public static class DockerImageMockData
    {
        public static List<DockerImageDto> GetMockDockerImages()
        {
            return
            [
                new DockerImageDto
                {
                    Title = "Ubuntu",
                    Description = "Official Ubuntu Docker Image",
                    LikesCount = 1200,
                    DownloadsCount = 500000,
                    LogoIcon = "icons8-docker-64.png"
                },
                new DockerImageDto
                {
                    Title = "Node.js",
                    Description = "Official Node.js Docker Image",
                    LikesCount = 900,
                    DownloadsCount = 350000,
                    LogoIcon = "icons8-docker-64.png"
                },
                new DockerImageDto
                {
                    Title = "MySQL",
                    Description = "Official MySQL Docker Image",
                    LikesCount = 800,
                    DownloadsCount = 300000,
                    LogoIcon = "icons8-docker-64.png"
                }
            ];
        }
    }
}
