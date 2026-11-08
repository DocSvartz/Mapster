using Mapster.Adapters;

namespace Mapster
{
    public static class TypeAdapterSettingsFactory
    {
        /// <summary>
        /// Settings used for creating mapping rule <see cref="ReadOnlyTypeAdapterRule"/> in: 
        /// <see cref="BaseAdapter"/>,
        /// <see cref="ReadOnlyTypeAdapterRule"/>
        /// .
        /// </summary>
        public static TypeAdapterSettings GetReadOnlySettings => new TypeAdapterSettings() { IsReadOnly = true };

        /// <summary>
        /// Settings used for creating mapping rule <see cref="TypeAdapterRule"/> in: 
        /// <see cref="TypeAdapterConfig.NewConfig"/>, 
        /// <see cref="TypeAdapterConfig.ForType"/>, 
        /// <see cref="TypeAdapterConfig.ForDestinationType"/>
        /// <see cref="TypeAdapterSetters.DefaultOnlySetter"/>
        /// .
        /// </summary>
        internal static TypeAdapterSettings GetSettings => new TypeAdapterSettings() { IsReadOnly = false };

        /// <summary>
        /// Settings for Fallback rule in 
        /// <see cref="MapsterGlobalConfig.RulesTemplate"/> 
        /// </summary>
        internal static TypeAdapterSettings GetFallBackReadOnlySettings
        {
            get 
            {
                var settings = new TypeAdapterSettings()
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
                };
                settings.IsReadOnly = true;

                return settings;
            }
        }
    }
}
