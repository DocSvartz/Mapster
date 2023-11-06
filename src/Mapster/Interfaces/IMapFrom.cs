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


#if !NETSTANDARD2_1_OR_GREATER
public interface IMapFrom<TSource>
{
    public void ConfigureMapping(TypeAdapterConfig config);
    
}
public static class MapFromLegacyHelper
{
    public static void MapFromConfigureDefault<TSource>(this IMapFrom<TSource> mapFrom, TypeAdapterConfig config)
    {
        config.NewConfig(typeof(TSource), mapFrom.GetType());
    }

}

#endif



