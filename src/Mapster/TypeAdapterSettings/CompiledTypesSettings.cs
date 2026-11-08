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
                if (settings.SourceType == Types.Source 
                    && settings.DestinationType == Types.Destination)
                    StorageVersion = settings.StorageVersion;

                QuickHash += settings.StorageVersion;
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
                   StorageVersion == other.StorageVersion &&
                   QuickHash == other.QuickHash &&
                   Types.Equals(other.Types);
        }

        public override int GetHashCode()
        {
            int hashCode = 707580169;
            hashCode = hashCode * -1521134295 + StorageVersion.GetHashCode();
            hashCode = hashCode * -1521134295 + QuickHash.GetHashCode();
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
