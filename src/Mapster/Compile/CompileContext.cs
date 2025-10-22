using System.Collections.Generic;
using System.Linq.Expressions;
using Mapster.Models;

namespace Mapster
{
    public class CompileContext
    {
        public HashSet<TypeTuple> Running { get; } = new();
        public Stack<ITypeAdapterConfig> Configs { get; } = new();
        public ITypeAdapterConfig Config => Configs.Peek();
        public int? MaxDepth { get; set; }
        public int Depth { get; set; }
        public HashSet<ParameterExpression> ExtraParameters { get; } = new();

        internal bool IsSubFunction()
        {
            return MaxDepth.HasValue || ExtraParameters.Count > 0;
        }

        public CompileContext(ITypeAdapterConfig config)
        {
            Configs.Push(config);
        }
    }
}