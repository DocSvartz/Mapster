using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster
{
    public interface ISourceToDestinationTypeAdapterSettings: IDestinationTypeAdapterSettings
    {
        bool? ApplyCustomConverterFactoryNullPropagation { get; set; }
        bool? MapToTargetPrimitive { get; set; }
        Action<TypeAdapterConfig>? Fork { get; set; }
        List<ResolverSourceInput> ExtraSources { get; }
        List<TypeTuple> Includes { get; }
        Func<CompileArgument, LambdaExpression>? ConverterFactory { get; set; }
        Func<CompileArgument, LambdaExpression>? ConverterToTargetFactory { get; set; }
        object? GenerateMapper { get; set; }
        List<string> ReMapDestinationMembers { get; }
    }
}