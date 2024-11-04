namespace FakeHubApi.Mapper;

public interface IBaseMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
    TSource ReverseMap(TDestination destination);

}