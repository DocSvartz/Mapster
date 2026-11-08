using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mapster
{
    public partial class TypeAdapterConfigSnapshot : IEnumerable<TypeAdapterRule>, IReadOnlyCollection<TypeAdapterRule>
    {
        public int Count => DefaultSettingsRule is not null ? 1 : 0
                            + AdapterRules.Length
                            + RuleMap.Keys.Count();

        public IEnumerator<TypeAdapterRule> GetEnumerator()
        {
            foreach (var x in AdapterRules) yield return x;
            if (DefaultSettingsRule is not null) yield return DefaultSettingsRule;
            foreach (var x in RuleMap.Values) yield return x;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

namespace Mapster.Cache 
{
    public partial class SettingsCacheStorage : IEnumerable<TypeAdapterRule>, IReadOnlyCollection<TypeAdapterRule>
    {
        public int Count => DefaultSettingsRule is not null ? 1 : 0
                           + AdapterRules.Length
                           + RuleMap.Keys.Count();

        public IEnumerator<TypeAdapterRule> GetEnumerator()
        {
            foreach (var x in AdapterRules) yield return x;
            if (DefaultSettingsRule is not null) yield return DefaultSettingsRule;
            foreach (var x in RuleMap.Values) yield return x;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

