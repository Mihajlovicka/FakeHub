using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto
{
    public class RepositorySearchDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Badge> Badges { get; set; } = new List<Badge>();
        public string? AuthorName { get; set; }
        public List<int> AuthorUserIds { get; set; } = new();
        public List<int> AuthorOrganizationIds { get; set; } = new();
        public List<string> GeneralTerms { get; set; } = new();
    }
}
