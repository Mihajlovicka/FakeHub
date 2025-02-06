using FakeHubApi.Model.Entity;

namespace FakeHubApi.Model.Dto;

public class RepositoryDto
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsPrivate { get; set; }
    public RepositoryOwnedBy OwnedBy { get; set; }
    public int OwnerId { get; set; }
    public Badge Badge { get; set; }
}