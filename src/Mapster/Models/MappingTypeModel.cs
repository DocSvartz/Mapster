using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapster.Models
{
    public class MappingTypeModel : IEquatable<MappingTypeModel?>
    {
        internal Type Type { get; private set; }
        internal IReadOnlyCollection<CustomAttributeData> CustomAttributes { get; init; }
        internal IReadOnlyCollection<IMemberModelEx> Members { get; init; }

        public MappingTypeModel(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));

            CustomAttributes = type.GetAllTypes()
                .SelectMany(t => t.GetTypeInfo().GetCustomAttributesData()).ToArray();
        }

        public MappingTypeModel(Type type, IReadOnlyCollection<IMemberModelEx> members) : this(type)
        {
            Members = members;
        }

        public MappingTypeModel(Type type, IEnumerable<IMemberModelEx> members) : this(type, members.ToArray()) { }

        public MappingTypeModel(Type type, IReadOnlyCollection<CustomAttributeData> customAttributes, IReadOnlyCollection<IMemberModelEx> members) : this(type)
        {
            CustomAttributes = customAttributes;
            Members = members;
        }

        public MappingTypeModel(Type type, IEnumerable<CustomAttributeData> customAttributes, IEnumerable<IMemberModelEx> members):
            this(type, customAttributes.ToArray(), members.ToArray()) { }

        


        #region Equlas
        public override bool Equals(object? obj)
        {
            return Equals(obj as MappingTypeModel);
        }

        public bool Equals(MappingTypeModel? other)
        {
            return other is not null &&
                   EqualityComparer<Type>.Default.Equals(Type, other.Type);
        }

        public override int GetHashCode()
        {
            return 2049151605 + EqualityComparer<Type>.Default.GetHashCode(Type);
        }

        public static bool operator ==(MappingTypeModel? left, MappingTypeModel? right)
        {
            return EqualityComparer<MappingTypeModel>.Default.Equals(left, right);
        }

        public static bool operator !=(MappingTypeModel? left, MappingTypeModel? right)
        {
            return !(left == right);
        }
        #endregion Equlas
    }
}
