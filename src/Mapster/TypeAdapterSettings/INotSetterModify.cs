using Mapster.Enums;
using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster
{
    public interface INotSetterModify
    {
        bool? MapToPrimitive { get; set; }
        bool? SkipDestinationMemberCheck { get; set; }

        ProjectToTypeAutoMapping ProjectToTypeMapConfig { get; set; }
        Dictionary<string, UnaryExpression> ProjectToTypeResolvers { get; set; }
        List<Func<ResolverSourceInput, IMemberModel, MemberMapping, CompileArgument, bool>> ValueAccessingStrategies { get; }
    }
}
