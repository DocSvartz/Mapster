using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapster
{
    public static class ConfigSnapshotExtentions
    {
        
        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        public static void CompileProjection<TSnapshot>(this TSnapshot snapshot) where TSnapshot : BaseMapFuncProducer, ITypeAdapterConfigSnapshot
        {
            var keys = snapshot.RuleMap.Keys.ToList();
            foreach (var key in keys)
            {
                snapshot._projectionDict[key] = snapshot.CreateProjectionCallExpression(key);
            }
        }



    }
}
