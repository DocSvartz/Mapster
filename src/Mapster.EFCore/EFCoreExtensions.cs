using Mapster.Enums;
using Mapster.Models;
using Mapster.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster.EFCore
{
    public static class EFCoreExtensions
    {
        public static IQueryable<TDestination> EFCoreProjectToType<TDestination>(this IQueryable source, 
            TypeAdapterConfig? config = null, ProjectToTypeAutoMapping autoMapConfig = ProjectToTypeAutoMapping.WithoutCollections)
        {
            var allInclude = new IncludeVisitor();
            allInclude.Visit(source.Expression);

            if (config == null)
            {
                config = TypeAdapterConfig.GlobalSettings
                .Clone()
                .ForType(source.ElementType, typeof(TDestination))
                .Config;

                var mapTuple = new TypeTuple(source.ElementType, typeof(TDestination));

                TypeAdapterRule rule;
                config.RuleMap.TryGetValue(mapTuple, out rule);

                if(rule != null)
                {
                    rule.Settings.ProjectToTypeMapConfig = autoMapConfig;

                    foreach (var item in allInclude.IncludeExpression)
                    {
                        var find = rule.Settings.Resolvers.Find(x => x.SourceMemberName == item.Key);
                        if (find != null)
                        {
                            find.Invoker = (LambdaExpression)item.Value.Operand;
                            find.SourceMemberName = null;
                        }
                        else
                            rule.Settings.ProjectToTypeResolvers.TryAdd(item.Key, item.Value);
                    }
                }
            }
            else
            {
                config = config.Clone()
                    .ForType(source.ElementType, typeof(TDestination))
                    .Config;
            }
            
            return source.ProjectToType<TDestination>(config);
        }
    }


    internal class IncludeVisitor : ExpressionVisitor
    {
        public Dictionary<string, UnaryExpression> IncludeExpression { get; protected set; } = new();
        private bool IsInclude(Expression node) => node.Type.Name.StartsWith("IIncludableQueryable");

        [return: NotNullIfNotNull("node")]
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            switch (node.NodeType)
            {
                case ExpressionType.Call:
                    {
                        if (IsInclude(node))
                        {
                            var QuoteVisiter = new QuoteVisitor();
                            QuoteVisiter.Visit(node);

                            foreach (var item in QuoteVisiter.Quotes)
                            {
                                var memberv = new TopLevelMemberNameVisitor();
                                memberv.Visit(item);

                                IncludeExpression.TryAdd(memberv.MemeberName, item);
                            }
                        }
                        return base.Visit(node);
                    }
            }

            return base.Visit(node);
        }
    }

}
