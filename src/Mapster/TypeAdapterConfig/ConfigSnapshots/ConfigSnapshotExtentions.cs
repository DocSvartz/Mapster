using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mapster.TypeAdapterConfig.ConfigSnapshots
{
    public static class ConfigSnapshotExtentions
    {
        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="failFast">A boolean parameter that determines whether exceptions should be thrown immediately when mapping errors occur or whether to collect and aggregate them. The default value is true.</param>
        /// <exception cref="AggregateException"></exception>
        public static void Compile<TSnapshot>(this TSnapshot snapshot,  bool failFast = true) where TSnapshot : BaseMapFuncProducer, ITypeAdapterConfigSnapshot
        {
            var exceptions = new List<Exception>();
            var keys = snapshot.RuleMap.Keys.ToList();

            foreach (var key in keys)
            {
                try
                {
                    if (key.Source == typeof(void))
                        continue;

                    if (key.Source.ContainsGenericParameters || key.Destination.ContainsGenericParameters)
                        continue;

                    snapshot._mapDict[key] = snapshot.Compiler(snapshot.CreateMapExpression(key, MapType.Map));
                    snapshot._mapToTargetDict[key] = snapshot.Compiler(snapshot.CreateMapExpression(key, MapType.MapToTarget));
                }
                catch (Exception ex)
                {
                    if (failFast)
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

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
