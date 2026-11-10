using System;

namespace Mapster
{
    [Obsolete($"In Mapster 11 it will be an abstract class. Use {nameof(CustomAdapterRule)}")]
    public class TypeAdapterRule
    {
        public Func<PreCompileArgument, int?> Priority { get; set; }
        public TypeAdapterSettings Settings { get; protected set; }
    }

    public class CustomAdapterRule : TypeAdapterRule
    {
        internal CustomAdapterRule(Func<PreCompileArgument, int?> priority, TypeAdapterSettings settings)
        {
            Priority = priority;
            Settings = settings;
        }
        public CustomAdapterRule(Func<PreCompileArgument, int?> priority): this()
        {
            Priority = priority;
        }
        public CustomAdapterRule()
        {
            Settings = SettingsFactory.GetFinalizeSettings;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExplicitTypeAdapterRule : TypeAdapterRule
    {
        public ExplicitTypeAdapterRule(Func<PreCompileArgument, int?> priority) : this()
        {
            Priority = priority;
        }
        public ExplicitTypeAdapterRule(Func<PreCompileArgument, int?> priority, TypeAdapterSettings settings)
        {
            Priority = priority;
            Settings = settings;
        }

        public ExplicitTypeAdapterRule()
        {
            Settings = SettingsFactory.GetExplicitTypeSettings;
        }
    }
}