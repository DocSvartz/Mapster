using Mapster.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    public interface ITypeAdapterConfigSettings
    {
        bool ActivateCustomConvertersSrcNullPropagation { get; set; }
        bool AllowImplicitDestinationInheritance { get; set; }
        bool AllowImplicitSourceInheritance { get; set; }
        bool RequireDestinationMemberSource { get; set; }
        bool RequireExplicitMapping { get; set; }
        bool RequireExplicitMappingPrimitive { get; set; }
        bool SelfContainedCodeGeneration { get; set; }
        Func<LambdaExpression, Delegate> Compiler { get; set; }
        DefaultOnlySetter Default { get; }
    }
}
