namespace Mapster.Cache
{
    internal record CompiledSettingsCacheItem
        (CompiledTypesSettings? Map,
        CompiledTypesSettings? MapToTaget,
        CompiledTypesSettings? Projection,
        bool IsVariableWithMapType = true);

}
