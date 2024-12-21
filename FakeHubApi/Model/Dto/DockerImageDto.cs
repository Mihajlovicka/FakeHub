namespace FakeHubApi.Model.Dto;

public class DockerImageDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public int DownloadsCount { get; set; }
    public string LogoIcon { get; set; } = string.Empty;
}
