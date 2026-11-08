namespace Mapster.Cache
{
    internal record CompiledSettingsCacheItem
        (CompiledTypesSettings? Map,
        CompiledTypesSettings? MapToTaget,
        CompiledTypesSettings? Projection,
        bool IsVariableWithMapType = true)
    {
        internal bool TryGetValue(MapType mapType, out CompiledTypesSettings? settings)
        {
            if (mapType == MapType.Map && Map != null)
                settings = Map;
            else if (mapType == MapType.MapToTarget && MapToTaget != null)
                settings = MapToTaget;
            else if (mapType == MapType.Projection && Projection != null)
                settings = Projection;
            else
            {
                settings = null;
                return false;
            }

            return true;
        }
    };

}
