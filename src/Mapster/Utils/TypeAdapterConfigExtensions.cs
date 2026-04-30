using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapster.Utils;

public static class TypeAdapterConfigExtensions
{
    public static void ScanInheritedTypes(this TypeAdapterConfig config, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t =>
                t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
            .ToList();
        InterfaceDynamicMapper dynamicMapper = new(config, types);
        dynamicMapper.ApplyMappingFromAssembly();
    }

    internal static void ScanInheritedTypes(this TypeAdapterConfig config, List<Type> types)
    {
        types = types.Where(t =>
                t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
            .ToList();
        InterfaceDynamicMapper dynamicMapper = new(config, types);
        dynamicMapper.ApplyMappingFromAssembly();
    }

    public static bool HasRuleFor(this TypeAdapterConfig config, Type srcType, Type dstType) =>
        config.RuleMap.ContainsKey(new TypeTuple(srcType, dstType));

    public static (bool isSuccess,Type[]? Result) GetOpenGenericTypeParamsStubs(this Type type)
    {
        var genericParams = new List<Type>();

        List<(int Index, List<int> RefOthetConstr, List<Type> ExtConstrains)> ConstraintsWithConstrain = new();

        if (type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            var constraints = args.Where(x => x.IsGenericParameter)
                .Select(x=>x.GetGenericParameterConstraints()).ToList();

            if (!constraints.Any())
                return (true,type.GetGenericArguments());


            for (var i = 0; i < constraints.Count; i++)
            {
                if (constraints[i].Length == 0)
                {
                    genericParams.Add(typeof(object));
                    continue;
                }

                if (constraints[i].Any(x => x.IsGenericParameter))
                {
                    List<int> RefOthetConstr = new List<int>();
                    List<Type> ExtConstrains = new List<Type>();

                    foreach (var item in constraints[i])
                    {
                        if (item.IsGenericParameter)
                            RefOthetConstr.Add(Array.IndexOf(args, item));
                        else
                            ExtConstrains.Add(item);
                    }

                    ConstraintsWithConstrain.Add((i, RefOthetConstr, ExtConstrains));

                    genericParams.Add(typeof(Never));
                    continue;
                }


                if (constraints[i].Length == 1 && constraints[i][0] == typeof(ValueType))
                {
                    genericParams.Add(typeof(int));
                    continue;
                }
                if (constraints[i].Length == 1 && constraints[i][0].IsClass)
                {
                    if (constraints[i][0].IsAbstract)
                    {
                        genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(ValueType), null));
                        continue;
                    }
                        
                    genericParams.Add(constraints[i][0]);
                    continue;
                }


                if (constraints[i].Any(x => x == typeof(ValueType)))
                {
                    var cInterface = constraints[i].Where(x => x.IsInterface)
                        .GetFlattenedUniqueInterfaces();

                    genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(ValueType), cInterface));
                    continue;
                }

                if (constraints[i].Any(x => x.IsClass))
                {
                    var classConstraint = constraints[i].First(x=> x.IsClass);

                    var cInterface = constraints[i].Where(x => x.IsInterface)
                        .GetFlattenedUniqueInterfaces();

                    genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(classConstraint, cInterface));
                    continue;
                }
            }

        }

        if (ConstraintsWithConstrain.Count != 0)
        {
            foreach (var item in ConstraintsWithConstrain)
            {
                var result = item.RefOthetConstr
                    .Where(i => i >= 0 && i < genericParams.Count)
                    .Select(i => genericParams[i])
                    .Concat(item.ExtConstrains).ToList();

                if (result.Count == 1)
                    genericParams[item.Index] = result[0];
                else
                {
                    var NotParentClases = result.Where(x => x.IsClass)
                                                    .Except(result.Where(x => x.IsClass && x.BaseType == typeof(object)));
                    var maxParentInterfaces = result.Where(x => x.IsInterface)
                                                    .GroupBy(i => i.GetInterfaces().Length)
                                                    .OrderByDescending(g => g.Key)
                                                    .First().ToList();

                    var clasesImplimented = NotParentClases
                        .Where(classType =>
                         maxParentInterfaces
                         .Any(interfaceType => interfaceType.IsAssignableFrom(classType)));

                    

                    if(NotParentClases.Count() == 1)
                    {
                        Type resultType = NotParentClases.First();
                       
                        resultType = DynamicTypeGenerator.GetTypeWitInterface(resultType, maxParentInterfaces.GetFlattenedUniqueInterfaces());
                       

                        genericParams[item.Index] = resultType;
                    }

                }
            }
        }


        return (true,genericParams.ToArray());
    }



    public static IEnumerable<Type> GetFlattenedUniqueInterfaces(this IEnumerable<Type> interfaces)
    {
        if (interfaces == null) yield break;

        var visited = new HashSet<Type>(new GenericTypeDefinitionComparer());

        foreach (var startingInterface in interfaces)
        {
          
            if (!visited.Add(startingInterface))
                continue;

            var queue = new Queue<Type>();
            queue.Enqueue(startingInterface);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

               
                yield return current;

            
                foreach (var nestedInterface in current.GetInterfaces())
                {
              
                    if (visited.Add(nestedInterface))
                    {
                        queue.Enqueue(nestedInterface);
                    }
                }
            }
        }
    }





    internal static void ForDestinationTypeRegsiter(this TypeAdapterConfig cfg, Type[] destTypes)
    {
        foreach (var item in destTypes)
        {
            cfg.ForDestinationType(item).DirectAssignmentForSameType(true);
        }
    }


    public class GenericTypeDefinitionComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type? x, Type? y)
        {
            if (x == null || y == null) return false;
            if (x.IsGenericType && y.IsGenericType)
            {
                return x.GetGenericTypeDefinition() == y.GetGenericTypeDefinition();
            }
            return x == y;
        }
        public int GetHashCode(Type obj) => (obj.IsGenericType ? obj.GetGenericTypeDefinition() : obj).GetHashCode();
    }


}