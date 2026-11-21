using Mapster.Models;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System.Linq.Expressions;

namespace Mapster
{
    public class TypeAdapterConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        #region CacheFunc
        
        protected override void InvalidateGenericCacheMapFunc(TypeTuple tuple)
        {
            if (this.Config == MapsterGlobalConfig.GlobalSettings)
            {
                var field = typeof(TypeAdapter<,>).MakeGenericType(tuple.Source, tuple.Destination).GetField("Map");
                field!.SetValue(null, _mapDict[tuple]);
            }
        }

        protected override bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker)
        {
            if (this.Config == MapsterGlobalConfig.GlobalSettings)
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
