using Mapster.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        List<(Type parent, List<Type> Implemnets)> valuesConstr = new();


        if(type.IsGenericType)
        {
            var args = type.GetGenericArguments();
            
            if(args.Where(x => x.IsGenericParameter).Any())
            {
                var constrains = new List<Type[]>();

                foreach (var arg in args.Where(x => x.IsGenericParameter).Select(x=>x.GetGenericParameterConstraints())) 
                {
                    var constrParam = arg.UnpackGenericParameterConstraints();

                    // check generic recursive Constrains - not avalible create stub in Runtime
                    foreach (var generic in constrParam.Where(x=>x.IsGenericType))
                    {
                      var IsRecursiveLink =  generic.GetGenericArguments().UnpackGenericParameterConstraints()
                            .Where(x => x.IsClass && x != typeof(ValueType))
                            .Any(constrParam.Contains);

                        if (IsRecursiveLink)
                            return (false, null);

                    }
                    constrains.Add(constrParam);
                }


            }


            //foreach (var constrs in constraints)
            //{
            //    var c = constrs
            //            .Select(x => x)
            //            .SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x }).ToList();

            //    if (c.Any(x=> x.IsGenericType))
            //    {
            //        foreach (var item in c.Where(x=>x.IsGenericType))
            //        {

            //        }
            //    }
                






            //    foreach (var item in constrs)
            //    {
            //        if(item.IsGenericType)
            //        {
            //           var result = type.GetGenericConstrains();

            //            if(result != null)
            //            {
            //                foreach (var cx in constraints)
            //                {
            //                    genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(result?.Parent, result?.Implemnets, result?.SelfGenericImpl));
            //                }
            //            }
                            
            //            return (true,genericParams.ToArray());
            //        }
            //    }



            }


        //foreach (var constrain in constraints)
        //{
        //    //List<Type> types = constrain
        //    //    .SelectMany(x => x.IsGenericParameter
        //    //    ? x.GetGenericParameterConstraints() : new[] { x })
        //    //    .Where(x => !x.IsGenericType).ToList();


        //    var GenericType = constrain.Where(x => x.IsGenericType);

        //    foreach (var generic in GenericType)
        //    {
        //        var arguments = generic.GetGenericArguments();

        //        if (arguments.Any(x=>x.IsGenericParameter))
        //        {
        //            return (false, null);


        //            //var consgen = generic.GetGenericArguments()
        //            //    .SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x }).Concat(arguments);

        //            //if(consgen.Count() >= constraints.Count)
        //            //{
        //            //    var unpakGenParams = consgen
        //            //        .SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x })
        //            //        .Distinct();


        //            //    var c = unpakGenParams.Where( x=> x.IsClass && !x.IsGenericType).FirstOrDefault();
        //            //    var i = unpakGenParams.Where(x => x.IsInterface && !x.IsGenericType);
        //            //    var gen = unpakGenParams.Where(x => x.IsGenericType);

        //            //    if(c != null)
        //            //    {

        //            //    }
        //            //}


        //        }
        //        else
        //        {
        //            var result = generic.GetOpenGenericTypeParamsStubs();
        //            if (result.isSuccess)
        //                types.Add(generic.GetGenericTypeDefinition().MakeGenericType(result.Result));
        //            else
        //                return (false, null);
        //        }
        //    }




        //    if (types.Any())
        //    {
        //        var getclass = types.Where(x => x.IsClass).Distinct().FirstOrDefault();
        //        var getInterfaces = types.Where(x => x.IsInterface).Distinct();

        //        if (getclass == null)
        //            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(object), getInterfaces));
        //        else
        //            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(getclass, getInterfaces));
        //    }

        //}

            return (false, null);
        }




        //if (type.IsGenericType)
        //{
        //    var args = type.GetGenericArguments();
        //    var constraints = args.Where(x => x.IsGenericParameter)
        //        .Select(x=>x.GetGenericParameterConstraints()).ToList();

        //    if (!constraints.Any())
        //        return (true,type.GetGenericArguments());


        //    for (var i = 0; i < constraints.Count; i++)
        //    {
        //        if (constraints[i].Length == 0)
        //        {
        //            genericParams.Add(typeof(object));
        //            continue;
        //        }

        //        if(constraints[i].Any(x => x.IsGenericType))
        //        {
        //            List<Type> generics = new();

        //            foreach (var genconstrain in constraints[i].Where(x => x.IsGenericType))
        //            {
        //                var s = genconstrain.GetOpenGenericTypeParamsStubs();
        //            }
        //        }


        //        if (constraints[i].Any(x => x.IsGenericParameter))
        //        {
        //            List<int> RefOthetConstr = new List<int>();
        //            List<Type> ExtConstrains = new List<Type>();

        //            List<Type> types = new List<Type>();



        //            foreach (var item in constraints[i])
        //            {
        //                types.AddRange(item.GetInterfaces());

        //               var consrtype = item.BaseType == typeof(object) ? null : item.BaseType;

        //                while (type.BaseType != null)
        //                {
        //                    types.Add(consrtype);
        //                }


        //                if (item.IsGenericParameter)
        //                    RefOthetConstr.Add(Array.IndexOf(args, item));
        //                else
        //                    ExtConstrains.Add(item);
        //            }

        //            ConstraintsWithConstrain.Add((i, RefOthetConstr, ExtConstrains));

        //            genericParams.Add(typeof(Never));
        //            continue;
        //        }


        //        if (constraints[i].Length == 1 && constraints[i][0] == typeof(ValueType))
        //        {
        //            genericParams.Add(typeof(int));
        //            continue;
        //        }
        //        if (constraints[i].Length == 1 && constraints[i][0].IsClass)
        //        {
        //            if (constraints[i][0].IsAbstract)
        //            {
        //                genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(ValueType), null));
        //                continue;
        //            }
                        
        //            genericParams.Add(constraints[i][0]);
        //            continue;
        //        }


        //        if (constraints[i].Any(x => x == typeof(ValueType)))
        //        {
        //            var cInterface = constraints[i].Where(x => x.IsInterface)
        //                .GetFlattenedUniqueInterfaces();

        //            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(ValueType), cInterface));
        //            continue;
        //        }

        //        if (constraints[i].Any(x => x.IsClass))
        //        {
        //            var classConstraint = constraints[i].First(x => x.IsClass);

        //            var cInterface = constraints[i].Where(x => x.IsInterface)
        //                .GetFlattenedUniqueInterfaces();

        //            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(classConstraint, cInterface));
        //            continue;
        //        }
        //        else if (constraints[i].All(x => x.IsInterface))
        //        {
        //            var cInterface = constraints[i].Where(x => x.IsInterface)
        //                .GetFlattenedUniqueInterfaces();

        //            genericParams.Add(DynamicTypeGenerator.GetTypeWitInterface(typeof(object), cInterface));
        //            continue;
        //        }
        //        else
        //            return (false, null);
        //    }

        //}

        //if (ConstraintsWithConstrain.Count != 0)
        //{
        //    foreach (var item in ConstraintsWithConstrain)
        //    {
        //        var result = item.RefOthetConstr
        //            .Where(i => i >= 0 && i < genericParams.Count)
        //            .Select(i => genericParams[i])
        //            .Concat(item.ExtConstrains)
        //            .Distinct().ToList();

        //        if (result.Count() == 1)
        //            genericParams[item.Index] = result[0];
        //        else
        //        {
        //            var clases = result.Where(x => x.IsClass);
        //            var NotParentClases = !clases.Any() || clases.Take(2).Count() == 1 ? clases : 
        //                result.Where(x => x.IsClass).Except(result.Where(x => x.IsClass && x.BaseType == typeof(object)));
        //            var maxParentInterfaces = result.Where(x => x.IsInterface).GetFlattenedUniqueInterfaces();

        //            var clasesImplimented = NotParentClases
        //                .Where(classType =>
        //                 maxParentInterfaces
        //                 .Any(interfaceType => interfaceType.IsAssignableFrom(classType)));


        //            if (!NotParentClases.Any())
        //                genericParams[item.Index] = DynamicTypeGenerator.GetTypeWitInterface(typeof(object), maxParentInterfaces);
        //            else if(NotParentClases.Take(2).Count() == 1)
        //            {
        //                Type resultType = NotParentClases.First();
        //                resultType = DynamicTypeGenerator.GetTypeWitInterface(resultType, maxParentInterfaces.GetFlattenedUniqueInterfaces());
        //                genericParams[item.Index] = resultType;
        //            }
        //            else
        //                return (false, null);

        //        }
        //    }
        //}


       // return (true,genericParams.ToArray());
    //}




    public static (Type Parent, IEnumerable<Type> Implemnets, IEnumerable<Type> SelfGenericImpl)? GetGenericConstrains(this Type type)
    {
        List<(Type parent, List<Type> Implemnets)> valuesConstr = new();


        if (type.IsGenericType)
        {
            var constraints = type.GetGenericArguments()
                 .SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x })
                 .Select(x => x)
                 .SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x })
                 .Distinct();

            var parentClass = constraints.Where(x => x.IsClass).FirstOrDefault();
            var implements = constraints.Where(x => x.IsInterface).GetFlattenedUniqueInterfaces();

            if (parentClass != null)
                return (parentClass, implements.Where(x => !x.IsGenericType), implements.Where(x => x.IsGenericType));
            else
                return (typeof(object), implements.Where(x => !x.IsGenericType), implements.Where(x => x.IsGenericType));

        }

        return null;
    }

    public static Type GenericParamsSelpReplaser(this Type generic, Type self)
    {
        var paramsT = new List<Type>();

        if (generic.IsGenericType)
        {
            var p = generic.GetGenericArguments();

            foreach ( var x in p)
            {
                if(x.IsGenericParameter)
                    paramsT.Add(self);
                else
                    paramsT.Add(x);
            }
        }

        return generic.GetGenericTypeDefinition().MakeGenericType(paramsT.ToArray());
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


    public static Type[] UnpackGenericParameterConstraints(this Type[]? constrains)
    {
        if (constrains == null)
            return new Type[0];

        List<Type> typeconstrains = new();

        do
        {
            if (typeconstrains.Any())
                typeconstrains = new(typeconstrains.SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x }));
            else
                typeconstrains = new(constrains.SelectMany(x => x.IsGenericParameter ? x.GetGenericParameterConstraints() : new[] { x }));

            var generics = typeconstrains.Select(x => x.IsGenericType);
        }
        while (typeconstrains.Any(x => x.IsGenericParameter));

        return typeconstrains.ToArray();
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