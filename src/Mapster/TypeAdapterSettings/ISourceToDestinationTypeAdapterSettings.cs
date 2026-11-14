using Mapster.MappingModels;
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
        Action<ITypeAdapterConfig>? Fork { get; set; }
        
        List<TypeTuple> Includes { get; }
        List<TypeTuple> Inherits { get; }
        Func<CompileArgument, LambdaExpression>? ConverterFactory { get; set; }
        Func<CompileArgument, LambdaExpression>? ConverterToTargetFactory { get; set; }
        object? GenerateMapper { get; set; }
        List<string> ReMapDestinationMembers { get; }

        internal TypeConverterModel TypeConverter { get; set; }
        public ParameterExpression Source { get; set; }
        public ParameterExpression Destination { get; set; }
        public ParameterExpression Result { get; set; }
    }
}