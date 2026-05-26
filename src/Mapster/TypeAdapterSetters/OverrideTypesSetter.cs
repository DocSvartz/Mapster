using Mapster.Utils;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class OverrideTypesSetter : TypeAdapterSetter
    {
        protected OverrideTypesSettings _Settings { get => (OverrideTypesSettings)Settings; }

        public OverrideTypesSetter() : this (new OverrideTypesSettings (), null) { }
        public OverrideTypesSetter(TypeAdapterSettings settings, TypeAdapterConfig config) : base(settings, config) { }
    }

    public class OverrideTypesSetter<TSource, TDestination> : OverrideTypesSetter
    {
        public OverrideTypesSetter<TSource, TDestination> SkipSettings(params Expression<Func<TypeAdapterSettings, object>>[] settings)
        {
            foreach (var member in settings)
            {
                _Settings.DropSettings.Add(member.GetMemberPath()!);
            }

            return this;
        }

        public TypeAdapterSetter<TSource, TDestination> ReConfigurate()
        {
            return new TypeAdapterSetter<TSource, TDestination>(this.Settings,this.Config);
        }
    }

    
}
