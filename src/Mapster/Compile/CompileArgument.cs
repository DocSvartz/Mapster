using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster
{
    public class CompileArgument
    {
        public Type SourceType { get; set; }
        public Type DestinationType { get; set; }
        public MapType MapType { get; set; }
        public bool ExplicitMapping { get; set; }
        public TypeAdapterSettings Settings { get; set; }
        public CompileContext Context { get; set; }
        public bool UseDestinationValue { get; set; }

        private HashSet<string>? _srcNames;
        internal HashSet<string> GetSourceNames()
        {
            return _srcNames ??= (from it in Settings.Resolvers
                                  where it.SourceMemberPath != null
                                     && it.SourceMemberPath.Length > 0
                                  select it.SourceMemberPath![0]).ToHashSet();
        }

        private HashSet<string>? _destNames;
        internal HashSet<string> GetDestinationNames()
        {
            return _destNames ??= (from it in Settings.Resolvers
                                   where it.DestinationMemberPath != null
                                      && it.DestinationMemberPath.Length > 0
                                   select it.DestinationMemberPath[0]).ToHashSet();
        }

        private bool _fetchConstructUsing;
        private LambdaExpression? _constructUsing;
        internal LambdaExpression? GetConstructUsing()
        {
            if (_fetchConstructUsing) return _constructUsing;
            _constructUsing = Settings.ConstructUsingFactory?.Invoke(this);
            _fetchConstructUsing = true;
            return _constructUsing;
        }
    }
}