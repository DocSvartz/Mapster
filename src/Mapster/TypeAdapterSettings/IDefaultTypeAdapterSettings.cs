using Mapster.Enums;
using Mapster.Models;
using System;
using System.Collections.Generic;

namespace Mapster
{
    /// <summary>
    /// Settings that can be safely modified, regardless of the Source or Destination types
    /// </summary>
    public interface IDefaultTypeAdapterSettings : ITypeAdapterSettingsIdentity, INotSetterModify
    {
        bool? AvoidInlineMapping { get; set; }
        bool? DirectAssignmentForSameType { get; set; }
        bool? EnableNonPublicConstructors { get; set; }
        bool? EnableNonPublicMembers { get; set; }
        bool? IgnoreNonMapped { get; set; }
        bool? IgnoreNullValues { get; set; }
        bool? IsLegasyIgnoreParameterNameMatching { get; set; }
        bool? MapEnumByName { get; set; }
        bool? PreserveReference { get; set; }
        bool? ShallowCopyForSameType { get; set; } // maybe start with destination setter level ?
        bool? Unflattening { get; set; } // maybe start with  destination setter level ?
        
        object? MapToConstructor { get; set; }
        int? MaxDepth { get; set; }
        DestinationMemebers RequireDestinationMemberSource { get; set; }
        NameMatchingStrategy NameMatchingStrategy { get; set; }
        List<Func<IMemberModel, bool>> UseDestinationValues { get; }
        List<Func<IMemberModel, MemberSide, string?>> GetMemberNames { get; }
        List<Func<IMemberModel, MemberSide, bool?>> ShouldMapMember { get; }
        List<DestinationTransform> DestinationTransforms { get; }
    }
}
