using FakeHubApi.ContainerRegistry;
using FakeHubApi.Model.Dto;

namespace FakeHubApi.Mapper.ArtifactMapper;

public class HarborArtifactToArtifactDtoMapper() : BaseMapper<HarborArtifact, ArtifactDto>
{
    public override ArtifactDto Map(HarborArtifact source)
    {
        return new ArtifactDto()
        {
            Id = source.Id,
            RepositoryName = source.RepositoryName,
            Tags = source.Tags.Select(t => new TagDto
            {
                Name = t.Name,
                Id = t.Id,
                PullTime = t.PullTime,
                PushTime = t.PushTime
            }).ToList(),
            ExtraAttrs = new ExtraAttrsDto
            {
                Os = source.ExtraAttrs.Os
            }
        };
    }

}