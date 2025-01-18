using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto;

public class TagDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime? PushTime { get; set; } = null;
    public DateTime? PullTime { get; set; } = null;
}