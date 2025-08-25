namespace FakeHubApi.Model.Dto;

public class EditRepositoryDto(int id, string description, bool isPrivate)
{
    public int Id { get; } = id;
    public bool IsPrivate { get;} = isPrivate;
    public string Description { get; } = description;
}