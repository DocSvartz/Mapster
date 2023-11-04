namespace Mapster;


#if NET6_0_OR_GREATER
public interface IMapFrom<TSource>
{

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig(typeof(TSource), GetType());
    }

}
#endif