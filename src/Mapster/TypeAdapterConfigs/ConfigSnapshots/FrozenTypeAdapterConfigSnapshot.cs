using Mapster.Models;
using Mapster.TypeAdapterConfigs.Builders;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    public class FrozenTypeAdapterConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        internal FrozenTypeAdapterConfigSnapshot() { }
        internal FrozenTypeAdapterConfigSnapshot(FrozenTypeAdapterConfigSnapshot old, BuilderTypeAdapterConfig patch) 
        {

        }

        protected override Expression CreateSelfExpression()
        {
            throw new NotImplementedException();
        }

        protected override void InvalidateGenericCacheMapFunc(TypeTuple tuple)
        {
            throw new NotImplementedException();
        }

        protected override bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker)
        {
            throw new NotImplementedException();
        }
    }
}
