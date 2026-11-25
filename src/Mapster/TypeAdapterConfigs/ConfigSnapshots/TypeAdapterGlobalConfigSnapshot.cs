using Mapster.Models;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System.Linq.Expressions;

namespace Mapster
{
    public sealed class TypeAdapterGlobalConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        private static readonly Expression _self = 
           Expression.Property(
               Expression.Property(null, typeof(MapsterGlobalConfig).GetProperty(nameof(MapsterGlobalConfig.GlobalSettings))!), 
               nameof(MapsterGlobalConfig.GlobalSettings.GetGlobalSnapshot));

        #region CacheFunc
        protected override Expression CreateSelfExpression() => _self;
        
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
