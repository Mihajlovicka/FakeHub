using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto;

public class ArtifactDto
{
    public int? Id { get; set; }
    public string RepositoryName { get; set; } = "";
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
    public ExtraAttrsDto ExtraAttrs { get; set; } = new ExtraAttrsDto();

}