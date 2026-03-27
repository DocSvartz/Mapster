using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Mapster.Utils;

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
        public bool? ConstructorMapping { get; set; }

        private HashSet<string>? _srcNames;
        internal HashSet<string> GetSourceNames()
        {
            return _srcNames ??= new HashSet<string>(
                from it in Settings.Resolvers
                where it.SourceMemberName != null
                select it.SourceMemberName!.Split('.').First(),
                StringComparer.Ordinal);
        }

        private HashSet<string>? _destNames;
        internal HashSet<string> GetDestinationNames(bool split = true)
        {
            return _destNames ??= new HashSet<string>(
                from it in Settings.Resolvers
                where it.DestinationMemberName != null
                select split
                    ? it.DestinationMemberName.Split('.').First()
                    : it.DestinationMemberName,
                StringComparer.Ordinal);
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

        public CompileArgument CloneWith(MapType? mapType = null)
        {
            var config = new TypeAdapterConfig();
            config.Default.EnableNonPublicMembers(true);
            config.Default.ShallowCopyForSameType(true);

            var str = this.BuildAdapter(config).CreateMapExpression<CompileArgument>();
            CompileArgument result = this.Adapt<CompileArgument>(config);

            if (mapType != null)
                result.MapType = mapType.Value;           
            
            return result;
        }
    }
}
