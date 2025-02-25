using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Mapster.Utils;

namespace Mapster
{
    public class IgnoreDictionary : ConcurrentDictionary<string[], IgnoreDictionary.IgnoreItem>, IApplyable<IgnoreDictionary>
    {
        public readonly struct IgnoreItem
        {
            public IgnoreItem(LambdaExpression? condition, bool isChildPath)
            {
                Condition = condition;
                IsChildPath = isChildPath;
            }

            public LambdaExpression? Condition { get; }
            public bool IsChildPath { get; }
        }

        public IgnoreDictionary() : base(new StringArrayEqualityComparer()) { }

        public void Apply(object other)
        {
            if (other is IgnoreDictionary collection)
                Apply(collection);
        }

        public void Apply(IgnoreDictionary other)
        {
            foreach (var member in other)
            {
                Merge(member.Key, member.Value);
            }
        }

        internal void Merge(string[] path, in IgnoreItem src)
        {
            if (src.Condition != null && TryGetValue(path, out var item))
            {
                if (item.Condition == null)
                    return;

                var param = src.Condition.Parameters.ToArray();
                var body = item.IsChildPath ? item.Condition.Body : item.Condition.Apply(param[0], param[1]);
                var condition = Expression.Lambda(Expression.OrElse(src.Condition.Body, body), param);

                TryUpdate(path, new IgnoreItem(condition, src.IsChildPath), item);
            }
            else
                TryAdd(path, src);

        }

        internal IgnoreDictionary Next(ParameterExpression source, ParameterExpression? destination, string destMemberName)
        {
            var result = new IgnoreDictionary();
            foreach (var member in this)
            {
                if (member.Key.Length <= 1 || member.Key[0] != destMemberName)
                    continue;

                var condition = member.Value.IsChildPath || member.Value.Condition == null
                    ? member.Value.Condition
                    : Expression.Lambda(member.Value.Condition.Apply(source, destination), source, destination);

                var next = new IgnoreItem(condition, true);
                result.Merge(member.Key[1..], next);
            }

            return result;
        }
    }
}
