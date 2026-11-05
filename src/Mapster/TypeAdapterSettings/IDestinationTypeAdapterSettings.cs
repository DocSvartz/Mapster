using Mapster.Enums;
using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster
{
    public interface IDestinationTypeAdapterSettings : IDefaultTypeAdapterSettings
    {
        bool? SkipDestinationMemberCheck { get; set; }

        List<Func<CompileArgument, LambdaExpression>> AfterMappingFactories { get; }
        List<Func<CompileArgument, LambdaExpression>> BeforeMappingFactories { get; }
        Func<CompileArgument, LambdaExpression>? ConstructUsingFactory { get; set; }
        Expression? CustomDefaultValue { get; set; }
        Action<TypeAdapterConfig>? Fork { get; set; }
        List<Func<IMemberModel, MemberSide, string?>> GetMemberNames { get; }
        IgnoreDictionary Ignore { get; }
        List<string> ReMapDestinationMembers { get; }
        List<string> UseDestinationMembers { get; }
        List<Func<IMemberModel, bool>> UseDestinationValues { get; }
        ProjectToTypeAutoMapping ProjectToTypeMapConfig { get; set; }

    }
}
