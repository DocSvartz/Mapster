using System;
using System.Diagnostics.CodeAnalysis;
using Mapster.Utils;

namespace Mapster
{
    public class NameMatchingStrategy: IApplyable<NameMatchingStrategy>
    {
        public Func<string, string> SourceMemberNameConverter { get; set; }
        public Func<string, string> DestinationMemberNameConverter { get; set; }
        public Func<string, string> SourceMemberToParameterNameConverter { get; set; }
        public Func<string, string> ParameterNameConverter { get; set; }

        public void Apply(object other)
        {
            if (other is NameMatchingStrategy strategy)
                Apply(strategy);
        }

        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition")]
        public void Apply(NameMatchingStrategy other)
        {
            SourceMemberNameConverter ??= other.SourceMemberNameConverter;
            DestinationMemberNameConverter ??= other.DestinationMemberNameConverter;
            SourceMemberToParameterNameConverter ??= other.SourceMemberToParameterNameConverter;
            ParameterNameConverter ??= other.ParameterNameConverter;
        }

        public static readonly NameMatchingStrategy Exact = new NameMatchingStrategy
        {
            SourceMemberNameConverter = MapsterHelper.Identity,
            DestinationMemberNameConverter = MapsterHelper.Identity,
            SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
            ParameterNameConverter = MapsterHelper.PascalCase
        };

        public static readonly NameMatchingStrategy Flexible = new NameMatchingStrategy
        {
            SourceMemberNameConverter = MapsterHelper.PascalCase,
            DestinationMemberNameConverter = MapsterHelper.PascalCase,
            SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
            ParameterNameConverter= MapsterHelper.PascalCase

        };

        public static readonly NameMatchingStrategy IgnoreCase = new NameMatchingStrategy
        {
            SourceMemberNameConverter = MapsterHelper.LowerCase,
            DestinationMemberNameConverter = MapsterHelper.LowerCase,
            SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
            ParameterNameConverter = MapsterHelper.PascalCase
        };

        public static readonly NameMatchingStrategy ToCamelCase = new NameMatchingStrategy
        {
            SourceMemberNameConverter = MapsterHelper.CamelCase,
            DestinationMemberNameConverter = MapsterHelper.Identity,
            SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
        };

        public static readonly NameMatchingStrategy FromCamelCase = new NameMatchingStrategy
        {
            SourceMemberNameConverter = MapsterHelper.Identity,
            DestinationMemberNameConverter = MapsterHelper.CamelCase,
            SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
        };

        public static NameMatchingStrategy ConvertSourceMemberName(Func<string, string> nameConverter)
        {
            return new NameMatchingStrategy
            {
                SourceMemberNameConverter = nameConverter,
                DestinationMemberNameConverter = MapsterHelper.Identity,
                SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
                ParameterNameConverter = MapsterHelper.PascalCase
            };
        }

        public static NameMatchingStrategy ConvertDestinationMemberName(Func<string, string> nameConverter)
        {
            return new NameMatchingStrategy
            {
                SourceMemberNameConverter = MapsterHelper.Identity,
                DestinationMemberNameConverter = nameConverter,
                SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
                ParameterNameConverter = MapsterHelper.PascalCase
            };
        }

        public static NameMatchingStrategy ConvertSourceMemberToParameterMemberName(Func<string, string> nameConverter)
        {
            return new NameMatchingStrategy
            {
                SourceMemberNameConverter = MapsterHelper.Identity,
                DestinationMemberNameConverter = MapsterHelper.Identity,
                SourceMemberToParameterNameConverter = nameConverter,
                ParameterNameConverter = MapsterHelper.PascalCase
            };
        }

        public static NameMatchingStrategy ConvertParameterMemberName(Func<string, string> nameConverter)
        {
            return new NameMatchingStrategy
            {
                SourceMemberNameConverter = MapsterHelper.Identity,
                DestinationMemberNameConverter = MapsterHelper.Identity,
                SourceMemberToParameterNameConverter = MapsterHelper.PascalCase,
                ParameterNameConverter = nameConverter
            };
        }

        public static Func<string, string> Identity = MapsterHelper.Identity;

        internal static Func<string, string> PascalCase = MapsterHelper.PascalCase;
        internal static Func<string, string> CamelCase = MapsterHelper.CamelCase;
        internal static Func<string, string> LowerCase = MapsterHelper.LowerCase;
    }
}
