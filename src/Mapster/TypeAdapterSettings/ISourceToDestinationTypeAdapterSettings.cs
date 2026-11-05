using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster
{
    public interface ISourceToDestinationTypeAdapterSettings: IDestinationTypeAdapterSettings
    {
        List<InvokerModel> Resolvers { get; }
        List<ResolverSourceInput> ExtraSources { get; }
        List<TypeTuple> Includes { get; }
        Func<CompileArgument, LambdaExpression>? ConverterFactory { get; set; }
        Func<CompileArgument, LambdaExpression>? ConverterToTargetFactory { get; set; }
        object? GenerateMapper { get; set; }
        Dictionary<string, UnaryExpression> ProjectToTypeResolvers { get; set; }
    }
}