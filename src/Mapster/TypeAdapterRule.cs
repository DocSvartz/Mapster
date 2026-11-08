using System;

namespace Mapster
{
    public class TypeAdapterRule
    {
        public TypeAdapterRule(Func<PreCompileArgument, int?> priority, TypeAdapterSettings settings)
        {
            Priority = priority;
            Settings = settings;
        }
        public TypeAdapterRule(Func<PreCompileArgument, int?> priority) : this()
        {
            Priority = priority;
        }
        public TypeAdapterRule()
        {
            Settings = TypeAdapterSettingsFactory.GetSettings;
        }

        [Obsolete("In Mapster 11 this Property will have init setter.")]
        public Func<PreCompileArgument, int?> Priority { get; set; }

        [Obsolete("In Mapster 11 this Property will have protected setter.")]
        public TypeAdapterSettings Settings { get; set; }
    }

    /// <summary>
    /// This class contains 
    /// <see cref="TypeAdapterSettings"/> settings in a ReadOnly state.<br/>
    /// If the settings passed to the constructor are not read-only,<br/>
    /// it will be converted using <see cref="TypeAdapterSettings.ToReadOnlyAndDisposeThis"/>
    /// </summary>
    public class ReadOnlyTypeAdapterRule : TypeAdapterRule
    {
        public ReadOnlyTypeAdapterRule(Func<PreCompileArgument, int?> priority, TypeAdapterSettings settings)
        {
            Priority = priority;

            if(settings.IsReadOnly)
                Settings = settings;
            else
                Settings = settings.ToReadOnlyAndDisposeThis();
        }
        public ReadOnlyTypeAdapterRule(Func<PreCompileArgument, int?> priority): this()
        {
            Priority = priority;
        }
        public ReadOnlyTypeAdapterRule()
        {
            Settings = TypeAdapterSettingsFactory.GetReadOnlySettings;
        }
    }
}