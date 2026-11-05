using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster
{
    public interface IDestinationTypeAdapterSettings : IDefaultTypeAdapterSettings
    {
        Func<CompileArgument, LambdaExpression>? ConstructUsingFactory { get; set; }
        Expression? CustomDefaultValue { get; set; }
        IgnoreDictionary Ignore { get; }

        List<Func<CompileArgument, LambdaExpression>> AfterMappingFactories { get; }
        List<Func<CompileArgument, LambdaExpression>> BeforeMappingFactories { get; }
        List<string> UseDestinationMembers { get; }
        List<InvokerModel> Resolvers { get; }
    }
}
