using Mapster.Adapters;
using System.Collections.Generic;

namespace Mapster
{
    public static class TypeAdapterConfigFactory
    {
        public static List<TypeAdapterRule> RulesTemplate { get; } = CreateRuleTemplate();
        public static TypeAdapterConfig GlobalSettings { get; } = new TypeAdapterConfig();

        private static List<TypeAdapterRule> CreateRuleTemplate()
        {
            return new List<TypeAdapterRule>
            {
                new PrimitiveAdapter().CreateRule(),    //-200
                new ClassAdapter().CreateRule(),        //-150
                new RecordTypeAdapter().CreateRule(),   //-149
                new ReadOnlyInterfaceAdapter().CreateRule(), // -148
                new CollectionAdapter().CreateRule(),   //-125
                new DictionaryAdapter().CreateRule(),   //-124
                new ArrayAdapter().CreateRule(),        //-123
                new MultiDimensionalArrayAdapter().CreateRule(), //-122
                new ObjectAdapter().CreateRule(),       //-111
                new StringAdapter().CreateRule(),       //-110
                new EnumAdapter().CreateRule(),         //-109

                //fallback rules
                new TypeAdapterRule
                {
                    Priority = arg => -200,
                    Settings = new TypeAdapterSettings
                    {
                        //match exact name
                        NameMatchingStrategy = NameMatchingStrategy.Exact,
                        ShouldMapMember =
                        {
                            ShouldMapMember.IgnoreAdaptIgnore,      //ignore AdaptIgnore attribute
                            ShouldMapMember.AllowPublic,            //match public prop
                            ShouldMapMember.AllowAdaptMember,       //match AdaptMember attribute
                        },
                        GetMemberNames =
                        {
                            GetMemberName.AdaptMember,              //get name using AdaptMember attribute
                        },
                        UseDestinationValues =
                        {
                            UseDestinationValue.Attribute,
                        },
                        ValueAccessingStrategies =
                        {
                            ValueAccessingStrategy.CustomResolver,  //get value from Map
                            ValueAccessingStrategy.PropertyOrField, //get value from properties/fields
                            ValueAccessingStrategy.GetMethod,       //get value from get method
                            ValueAccessingStrategy.FlattenMember,   //get value from chain of properties
                        }
                    }
                },

                //dictionary accessor
                new TypeAdapterRule
                {
                    Priority = arg => arg.SourceType.GetDictionaryType()?.GetGenericArguments()[0] == typeof(string) ? DictionaryAdapter.DefaultScore : (int?)null,
                    Settings = new TypeAdapterSettings
                    {
                        ValueAccessingStrategies =
                        {
                            ValueAccessingStrategy.CustomResolverForDictionary,
                            ValueAccessingStrategy.Dictionary,
                        },
                    }
                }
            };
        }

    }
}
