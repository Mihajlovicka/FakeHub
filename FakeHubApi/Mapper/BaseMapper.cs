namespace FakeHubApi.Mapper;

public abstract class BaseMapper<TSource, TDestination> : IBaseMapper<TSource, TDestination>
{
    public abstract TDestination Map(TSource source);
    public virtual TSource ReverseMap(TDestination destination)
    {
        throw new NotImplementedException("Reverse mapping is not implemented.");
    }
}