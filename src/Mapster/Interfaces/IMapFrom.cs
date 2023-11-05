namespace Mapster;


#if NETSTANDARD2_1_OR_GREATER
public interface IMapFrom<TSource>
{

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig(typeof(TSource), GetType());
    }

}
#endif