namespace Mapster;

public interface IMapFrom<TSource>
{
#if NETSTANDARD2_0
    void ConfigureMapping(TypeAdapterConfig config);
#else
    void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig(typeof(TSource), GetType());
    }
#endif
}
