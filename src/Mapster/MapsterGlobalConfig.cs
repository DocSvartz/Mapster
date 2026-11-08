using Mapster.Adapters;
using System.Collections.Immutable;

namespace Mapster
{
    public static class MapsterGlobalConfig
    {
        public static ImmutableArray<TypeAdapterRule> RulesTemplate { get; } = RuleTemplateBuilder().ToImmutable();
        public static TypeAdapterConfig GlobalSettings { get; } = new TypeAdapterConfig();


        private static ImmutableArray<TypeAdapterRule>.Builder RuleTemplateBuilder()
        {
            var result = ImmutableArray.CreateBuilder<TypeAdapterRule>();

            result.Add(new PrimitiveAdapter().CreateRule());                //-210
            result.Add(new ClassAdapter().CreateRule());                    //-200
            result.Add(new NotSelfCreationAdapter().CreateRule());          //-150          
            result.Add(new RecordTypeAdapter2().CreateRule());              //-149
            result.Add(new ReadOnlyInterfaceAdapter().CreateRule());        //-148
            result.Add(new CollectionAdapter().CreateRule());               //-125
            result.Add(new DictionaryAdapter().CreateRule());               //-124
            result.Add(new ArrayAdapter().CreateRule());                    //-123
            result.Add(new MultiDimensionalArrayAdapter().CreateRule());    //-122
            result.Add(new ObjectAdapter().CreateRule());                   //-111
            result.Add(new StringAdapter().CreateRule());                   //-110
            result.Add(new EnumAdapter().CreateRule());                     //-109
            result.Add(new NullableAdapter().CreateRule());                 // 0

            //fallback rules
            result.Add(
                        new TypeAdapterRule
                        {
                            Priority = arg => -200,
                            Settings = new TypeAdapterSettings
                            {
                                SourceType = typeof(void),
                                DestinationType = typeof(void),

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
                        }

                );

            //dictionary accessor
            result.Add(
                    new TypeAdapterRule
                    {
                        Priority = arg => arg.SourceType.GetDictionaryType()?.GetGenericArguments()[0] == typeof(string) ? DictionaryAdapter.DefaultScore : (int?)null,
                        Settings = new TypeAdapterSettings
                        {
                            SourceType = typeof(DictionaryAdapter),
                            DestinationType = typeof(DictionaryAdapter),
                            ValueAccessingStrategies =
                        {
                            ValueAccessingStrategy.CustomResolverForDictionary,
                            ValueAccessingStrategy.Dictionary,
                        },
                        }
                    }
                );

           return result;
        }

    }
}
