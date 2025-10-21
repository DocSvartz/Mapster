using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapster
{
    public static class TypeAdapterConfigIRegisterExtentions
    {
        /// <summary>
        /// Applies type mappings.
        /// </summary>
        /// <param name="registers">IRegister interface params to apply mapping.</param>
        public static void Apply(this TypeAdapterConfig config, params IRegister[] registers)
        {
            foreach (IRegister register in registers)
            {
                register.Register(config);
            }
        }

        /// <summary>
        /// Applies type mappings.
        /// </summary>
        /// <param name="registers">collection of IRegister interface to apply mapping.</param>
        public static void Apply(this TypeAdapterConfig config, IEnumerable<Lazy<IRegister>> registers)
        {
            config.Apply(registers.Select(register => register.Value));
        }

        /// <summary>
        /// Collect IRegisters mappings from specified assemblies.
        /// </summary>
        /// <param name="assemblies">assemblies to scan.</param>
        /// <returns>A list of registered mappings</returns>
        public static IList<IRegister> Scan(this TypeAdapterConfig config, params Assembly[] assemblies)
        {
            List<IRegister> registers = assemblies.Select(assembly => assembly.GetLoadableTypes()
                .Where(x => typeof(IRegister).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo()) && x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract))
                .SelectMany(registerTypes =>
                    registerTypes.Select(registerType => (IRegister)Activator.CreateInstance(registerType))).ToList();

            config.Apply(registers);
            return registers;
        }
    }
}
