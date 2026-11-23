using Mapster.Models;
using Mapster.TypeAdapterConfigs;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System.Linq.Expressions;

namespace Mapster
{
    public class TypeAdapterConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        #region CacheFunc
        private SnapshotCacheMarker _cacheMarker = SnapshotCacheMarker.Attached;

        public SnapshotCacheMarker CacheMarker 
        { 
            get { 
                    if(_cacheMarker == SnapshotCacheMarker.Detached)
                        return _cacheMarker;
                    else if(_cacheMarker == SnapshotCacheMarker.Attached)
                    {
                        if (this.Config == MapsterGlobalConfig.GlobalSettings
                            && this.Version == MapsterGlobalConfig.GlobalSettings.MappingCache.Version);
                        else
                            _cacheMarker = SnapshotCacheMarker.Detached;
                    }
                return this._cacheMarker;
            } 
        }

        protected override void InvalidateGenericCacheMapFunc(TypeTuple tuple)
        {
            if (CacheMarker != SnapshotCacheMarker.Detached)
            {
                var field = typeof(TypeAdapter<,>).MakeGenericType(tuple.Source, tuple.Destination).GetField("Map");
                field!.SetValue(null, _mapDict[tuple]);
            }
        }

        protected override bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker)
        {
            if (CacheMarker != SnapshotCacheMarker.Detached)
            {
                var property = typeof(TypeAdapter<,>).MakeGenericType(tuple.Source, tuple.Destination).GetField("Map");
                invoker = Expression.Field(null, property);
                return true;
            }
            else
                invoker = null;
                
            return false;
        }

        #endregion CacheFunc

    }
}
