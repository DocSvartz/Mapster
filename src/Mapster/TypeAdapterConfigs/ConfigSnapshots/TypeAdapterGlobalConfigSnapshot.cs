using Mapster.Models;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System.Linq.Expressions;

namespace Mapster
{
    public sealed class TypeAdapterGlobalConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        #region CacheFunc
        
        protected override void InvalidateGenericCacheMapFunc(TypeTuple tuple)
        {
            var field = typeof(TypeAdapter<,>).MakeGenericType(tuple.Source, tuple.Destination).GetField("Map");
            field!.SetValue(null, _mapDict[tuple]);
        }

        protected override bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker)
        {
            var property = typeof(TypeAdapter<,>).MakeGenericType(tuple.Source, tuple.Destination).GetField("Map");
            invoker = Expression.Field(null, property);
            return true;
        }

        #endregion CacheFunc

    }
}
