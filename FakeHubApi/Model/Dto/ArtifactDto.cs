using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto;

public class ArtifactDto
{
    public int? Id { get; set; }
    public string Digest { get; set; } = "";
    public string RepositoryName { get; set; } = "";
    public TagDto Tag { get; set; } = new TagDto();
    public ExtraAttrsDto ExtraAttrs { get; set; } = new ExtraAttrsDto();

}