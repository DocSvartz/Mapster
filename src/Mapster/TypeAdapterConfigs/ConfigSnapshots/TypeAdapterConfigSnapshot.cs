using Mapster.Models;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System.Linq.Expressions;

namespace Mapster
{
    public class TypeAdapterConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        protected override Expression CreateSelfExpression()
        {
            return Expression.Constant(this);
        }

        protected override void InvalidateGenericCacheMapFunc(TypeTuple tuple)
        {
            // skip not access to global static cache 
        }

        protected override bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker)
        {
           invoker = null;
           return false;
        }
    }
}
