using Mapster.Models;
using System;
using System.Collections.Generic;

namespace Mapster
{
    internal class CompiledTypesSettings : TypeAdapterSettings, IEquatable<CompiledTypesSettings?>
    {
        public CompiledTypesSettings(TypeTuple types) : base()
        {
            Types = types;
        }

        internal TypeTuple Types { get; set; }

        public override void Apply(SettingStore other)
        {
            if(other is TypeAdapterSettings settings)
            {
                Id += settings.Id;
                Version += settings.Version;
            }
            base.Apply(other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CompiledTypesSettings);
        }

        public bool Equals(CompiledTypesSettings? other)
        {
            return other is not null &&
                   Id == other.Id &&
                   Version == other.Version &&
                   Types.Equals(other.Types);
        }

        public override int GetHashCode()
        {
            int hashCode = 707580169;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Version.GetHashCode();
            hashCode = hashCode * -1521134295 + Types.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CompiledTypesSettings? left, CompiledTypesSettings? right)
        {
            return EqualityComparer<CompiledTypesSettings>.Default.Equals(left, right);
        }

        public static bool operator !=(CompiledTypesSettings? left, CompiledTypesSettings? right)
        {
            return !(left == right);
        }
    }
}
