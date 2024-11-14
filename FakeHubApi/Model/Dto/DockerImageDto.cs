namespace FakeHubApi.Model.Dto
{
    public class DockerImageDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int LikesCount { get; set; }
        public int DownloadsCount { get; set; }
        public string LogoIcon { get; set; }
    }
}
