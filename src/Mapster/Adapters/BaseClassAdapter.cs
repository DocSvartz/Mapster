using Mapster.Enums;
using Mapster.MappingModels;
using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Mapster.Adapters
{
    internal abstract class BaseClassAdapter : BaseAdapter
    {
        protected override ObjectType ObjectType => ObjectType.Class;
        protected override bool UseTargetValue => true;

        #region Build the Adapter Model

        private List<MemberMapping> CreateSourceToDestinationConveter(Expression source, MembersModel classModel, CompileArgument arg)
        {
            var destinationMembers = classModel.Members;
            //var unmappedDestinationMembers = new List<string>();
            List<MemberMapping> members = new List<MemberMapping>();

            if (arg.Settings.IgnoreNonMapped == true)
                IgnoreNonMapped(classModel,arg);
          
            var sources = new List<ResolverSourceInput> {(ResolverSourceInput)source};
            sources.AddRange(
                arg.Settings.ExtraSources.Select(src => src.Activate(source, arg)));
            foreach (var destinationMember in destinationMembers)
            {
                ProcessIgnores(arg, destinationMember, out var ignore);

                // perf optimization. Backfields are not processed in the standard mapping pipeline.
                if (destinationMember.IsBackField && ignore.WithOutCondition) 
                    continue;

                var propertyModel = new MemberMapping();

                //If member has ignore without contition,
                //it canbe remap in using Custom mapping or flattening Custom mapping only
                if (ignore.WithOutCondition)
                {
                    var onlyCustomResolvers = arg.Settings.ValueAccessingStrategies.AsEnumerable().Where(ValueAccessingStrategy.RemapMaybeResolvers.Contains);
                    var resolver = (from fn in onlyCustomResolvers
                                    from src in sources
                                    select fn(src, destinationMember, propertyModel, arg))
                        .FirstOrDefault(result => result);
                }
                else
                {
                    var resolvers = arg.Settings.ValueAccessingStrategies.AsEnumerable();
                    if (arg.Settings.IgnoreNonMapped == true)
                        resolvers = resolvers.Where(ValueAccessingStrategy.CustomResolvers.Contains);
                    var resolver = (from fn in resolvers
                                    from src in sources
                                    select fn(src, destinationMember, propertyModel, arg))
                        .FirstOrDefault(result => result);
                }

                var nextIgnore = arg.Settings.Ignore.Next((ParameterExpression)source, (ParameterExpression?)arg.Destination, destinationMember.Name);
                var nextResolvers = arg.Settings.Resolvers.Next(arg.Settings.Ignore, (ParameterExpression)source, destinationMember.Name)
                    .ToList();

                propertyModel
                    .DestinationMember = destinationMember;
                propertyModel
                    .Ignore = ignore;
                propertyModel
                    .NextResolvers = nextResolvers;
                propertyModel.
                    NextResolvers = nextResolvers;
                propertyModel.
                    NextIgnore = nextIgnore;
                propertyModel.
                    Source = (ParameterExpression)source; //  must be determined for each GetterLine
                propertyModel.
                    Destination = arg.Destination;
                propertyModel.
                    Result = arg.Result;
                propertyModel.
                    UseDestinationValue = IsCanUsingDestinationValue(arg, destinationMember);


                if(arg.MapType == MapType.ApplyNullPropagation &&
                    propertyModel.IsNotFoundGetter  
                    && !arg.DestinationType.IsRecordType()  
                    && destinationMember.Info is PropertyInfo propinfo)
                {
                    if (propinfo.GetCustomAttributes()
                        .Any(y => y.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"))
                    {
                        propertyModel.GetterLines.Add(new GetterLine(source, destinationMember.Type.CreateDefault(arg), null));
                    }
                }
                //maybe refactoring to always create record restored member
                if (arg.DestinationType.IsRecordType())
                {
                    propertyModel
                        .RestoreDestinationMemberExp = TryRestoreRecordMember(destinationMember, arg);
                }
                if (propertyModel.GetterLines.Count != 0 || propertyModel.Ignore.IsValid
                    || propertyModel.RestoreDestinationMemberExp != null)
                {
                    propertyModel.CreateNullPropagationChecker(arg);
                    members.Add(propertyModel);
                }
                else
                {
                    if (arg.Settings.IgnoreNonMapped != true &&
                        arg.Settings.Unflattening == true &&
                        arg.DestinationType.GetDictionaryType() == null &&
                        arg.SourceType.GetDictionaryType() == null)
                    {
                        var extra = ValueAccessingStrategy.FindUnflatteningPairs(source, destinationMember, arg)
                            .Next(arg.Settings.Ignore, (ParameterExpression)source, destinationMember.Name);
                        nextResolvers.AddRange(extra);
                    }

                    if (classModel.ConstructorInfo != null)
                    {
                        var info = (ParameterInfo)destinationMember.Info!;
                        if (!info.IsOptional)
                        {
                           
                            //if(!arg.Settings.Ignore.Any(x=>x.Key == destinationMember.Name)) // Don't mark a constructor parameter if it was explicitly ignored
                            //    unmappedDestinationMembers.Add(destinationMember.Name);
                        }

                        members.Add(propertyModel);
                    }
                    else if (propertyModel.HasSettings())
                    {
                        propertyModel.GetterLines.Add(new GetterLine(source, Expression.New(typeof(Never)), null));
                        members.Add(propertyModel);
                    }
                    else if (destinationMember.UseDestinationValue(arg) || destinationMember.SetterModifier != AccessModifier.None)
                    {
                        
                        //unmappedDestinationMembers.Add(destinationMember.Name);

                        members.Add(propertyModel);
                    }
                }
            }

            //var requireDestMemberSource = arg.Settings.RequireDestinationMemberSource.HasPropertyOrFields()
            //    || arg.Context.Config.RequireDestinationMemberSource;
            //if (requireDestMemberSource &&
            //    unmappedDestinationMembers.Count > 0 &&
            //    arg.Settings.SkipDestinationMemberCheck != true)
            //{
            //    throw new InvalidOperationException($"The following members of destination class {arg.DestinationType} do not have a corresponding source member mapped or ignored:{string.Join(",", unmappedDestinationMembers)}");
            //}

            //if(classModel.ConstructorInfo is ConstructorInfo)
            //    return new ConstructorConveterModel() { Info = classModel.ConstructorInfo, Members = members };
            //if (classModel.ConstructorInfo is MethodBase)
            //    return new MetodConverterModel() { Info = classModel.ConstructorInfo, Members = members };
            //else
            //    return new PropertyOrFielldConveterModel() { Members = members };

            return members;
        }

        protected ConstructorConveterModel CreateSourceToDestinationConstructorConveter(Expression source, ConstructorInfo constructor, CompileArgument arg)
        {
            var memberModel = GetConstructorModel(constructor);
            return new ConstructorConveterModel() { Info = constructor, Members = CreateSourceToDestinationConveter(source, memberModel, arg) };
        }
        protected PropertyOrFielldConveterModel CreateSourceToDestinationPropertOrFieldConverter(Expression source, CompileArgument arg)
        {
            var memberModel = GetSetterModel(arg);
            return new PropertyOrFielldConveterModel() { Members = CreateSourceToDestinationConveter(source, memberModel, arg) };
        }

        protected static bool IsCanUsingDestinationValue(CompileArgument arg, IMemberModelEx destinationMember)
        {
            if (arg.MapType == MapType.Projection)
                return false;
            if (destinationMember.UseDestinationValue(arg) || arg.Settings.UseDestinationMembers.Contains(destinationMember.Name))
                return true;

            return false;
        }

        protected static void ProcessIgnores(
            CompileArgument arg,
            IMemberModel destinationMember, 
            out IgnoreDictionary.IgnoreItem ignore)
        {
            if (!destinationMember.ShouldMapMember(arg, MemberSide.Destination))
                ignore = new IgnoreDictionary.IgnoreItem(); // equal to set Ignore without condition;
            else
            {
                ignore = IgnoreDictionary.IgnoreItem.InvalidIgnore; // Ignore is not set

                if(arg.Settings.IsLegasyIgnoreParameterNameMatching == true && destinationMember.Info is ParameterInfo)
                {
                    if (arg.Settings.Ignore.TryGetValue(destinationMember.Name.ToPascalCase(), out var realIgnore))
                        ignore = realIgnore; // сonditional Ignore set
                }
                else
                {
                    if (arg.Settings.Ignore.TryGetValue(destinationMember.Name, out var realIgnore))
                        ignore = realIgnore; // сonditional Ignore set
                }
            }
        }

        protected Expression CreateInstantiationExpression(Expression source, ConstructorConveterModel classConverter, CompileArgument arg)
        {
            var members = classConverter.Members;
            var unmappedDestinationCtorParams = new List<string>();

            var arguments = new List<Expression>();

            foreach (var member in members)
            {
                var parameterInfo = (ParameterInfo)member.DestinationMember.Info!;
                Expression defaultConst;
                Expression getter;

#if NETSTANDARD2_0
                try
                {
                    defaultConst = parameterInfo.IsOptional && parameterInfo.DefaultValue != null
                    ? Expression.Constant(parameterInfo.DefaultValue, member.DestinationMember.Type)
                    : parameterInfo.ParameterType.CreateDefault(arg);
                }
                catch (FormatException)
                {
                    defaultConst = parameterInfo.ParameterType.CreateDefault(arg);
                }

#else
                defaultConst = parameterInfo.IsOptional && parameterInfo.DefaultValue != null
                    ? Expression.Constant(parameterInfo.DefaultValue, member.DestinationMember.Type)
                    : parameterInfo.ParameterType.CreateDefault(arg);
#endif
                
                if (member.IsNotFoundGetter)
                {
                    getter = defaultConst;

                    if (arg.MapType == MapType.MapToTarget && arg.DestinationType.IsRecordType())
                        getter = TryRestoreRecordMember(member.DestinationMember, arg) ?? getter;
                }
                else
                {
                    if (member.Ignore.WithOutCondition)
                    {
                        getter = defaultConst;

                        if (arg.MapType == MapType.MapToTarget && arg.DestinationType.IsRecordType())
                            getter = TryRestoreRecordMember(member.DestinationMember, arg) ?? getter;
                    }
                    else
                        getter = GetMemberInlineAdapter(member, arg);
                }
                arguments.Add(getter);

                if(member.IsNotFoundGetter && !member.Ignore.WithOutCondition)
                    unmappedDestinationCtorParams.Add(member.DestinationMember.Name);
            }

            if(arg.Settings.RequireDestinationMemberSource.HasConstrutorMembers())
            {

            }

            return Expression.New(classConverter.ConstructorInfo!, arguments);
        }

        protected virtual MembersModel GetConstructorModel(ConstructorInfo ctor)
        {
            return new MembersModel
            {
                ConstructorInfo = ctor,
                Members = ctor.GetParameters().Select(ReflectionUtils.CreateModel)
            };
        }

        protected virtual MembersModel GetSetterModel(CompileArgument arg)
        {
            return new MembersModel
            {
                Members = arg.DestinationType.GetFieldsAndProperties(arg,true)
            };
        }

        protected void IgnoreNonMapped (MembersModel classModel, CompileArgument arg)
        {
            var notMappingToIgnore = LinqCompat.ExceptBy(
                classModel.Members,
                arg.Settings.Resolvers.Select(x => x.DestinationMemberName),
                y => y.Name);

            foreach (var item in notMappingToIgnore)
            {
                if (!item.ShouldMapMember(arg, MemberSide.Destination))
                    continue;

                arg.Settings.Ignore.TryAdd(item.Name, new IgnoreDictionary.IgnoreItem());
            }
        }

        protected virtual MembersModel GetOnlyRequiredPropertySetterModel(CompileArgument arg)
        {
            return new MembersModel
            {
                Members = arg.DestinationType.GetFieldsAndProperties(arg,true)
                    .Where(x => x.GetType() == typeof(PropertyModel))
                    .Where(y => ((PropertyInfo)y.Info).GetCustomAttributes()
                    .Any(y => y.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"))
            };
        }

        protected Expression? TryRestoreRecordMember(IMemberModelEx member, CompileArgument arg)
        {
            arg.Context.Config.TypeMemberModelsCache.TryGetValue(arg.DestinationType, out var members);

            if (members != null && arg.MapType == MapType.MapToTarget)
            {
                var find = members
                               .Where(x => string.Equals(x.Name, member.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (find != null)
                {
                    var compareNull = Expression.Equal(arg.Destination, Expression.Constant(null, arg.Destination.Type));
                    return Expression.Condition(compareNull, member.Type.CreateDefault(arg), Expression.MakeMemberAccess(arg.Destination, (MemberInfo)find.Info));
                }

            }

            return null;
        }

        protected Expression? TryRestoreRecordMember(MemberMapping member, CompileArgument arg)
        {
            return TryRestoreRecordMember(member.DestinationMember, arg);
        }

        protected static Expression SetValueTypeAutoPropertyByReflection(MemberMapping member, Expression adapt, MembersModel checkmodel)
        {
            var modDesinationMemeberName = $"<{member.DestinationMember.Name}>k__BackingField";
            if (checkmodel.Members.Any(x => x.Name == modDesinationMemeberName) == false) // Property is not autoproperty
                return Expression.Empty();
            var typeofExpression = Expression.Constant(member.Destination!.Type);
            var getPropertyMethod = typeof(Type).GetMethod("GetField", new[] { typeof(string), typeof(BindingFlags) })!;
            var getPropertyExpression = Expression.Call(typeofExpression, getPropertyMethod,
                Expression.Constant(modDesinationMemeberName), Expression.Constant(BindingFlags.Instance | BindingFlags.NonPublic));
            var setValueMethod =
                typeof(FieldInfo).GetMethod("SetValue", new[] { typeof(object), typeof(object) })!;
            var memberAsObject = adapt.To(typeof(object));
            return Expression.Call(getPropertyExpression, setValueMethod,
                new[] { member.Destination, memberAsObject });
        }

        /// <summary>
        /// using for Map or Projection cases
        /// </summary>
        /// <param name="member"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Expression GetMemberInlineAdapter(MemberMapping member, CompileArgument arg, Expression? defaultValue = null)
        {
            Expression? result = null;
            Expression defaultcase = defaultValue;

            var modGetterLine = member.GetterLines.All(x => x.Condition == null) 
                ? member.GetterLines.Take(1).Select(x => x.ApplyContextSettings(member, arg)).ToArray()
                : member.GetterLines.Select(x => x.ApplyContextSettings(member, arg)).ToArray();
            var single = modGetterLine.GetIfSingle();

            if (single.getterLine != null && single.modCondition == null)
            {
                member.OverrideSettings = single.getterLine.OverrideSettings; // need refactoring

                if (arg.MapType == MapType.Projection)
                {
                    var test = single.getterLine.NullPropagationChecker.ApplyProjectionNullCheck(single.modgetter);
                    var adapt = CreateAdaptExpression(single.modgetter, member.DestinationMember.Type, arg, member);
                    var def = adapt.Type.CreateDefault();

                    return Expression.Condition(test, adapt, def);
                }    
                  
                return CreateAdaptExpression(single.modgetter, member.DestinationMember.Type, arg, member);
            }
               
            for (var i = 0; i < modGetterLine.Length; i++)
            {
                member.OverrideSettings = modGetterLine[i].getterLine.OverrideSettings; // need refactoring

                modGetterLine[i].TransformFunc = CreateAdaptExpression(modGetterLine[i].modgetter, member.DestinationMember.Type, arg, member);
            }

            if(defaultcase == null)
                defaultcase = member.RestoreDestinationMemberExp 
                    ?? modGetterLine.Where(x => x.modCondition == null).FirstOrDefault().TransformFunc ?? member.DestinationMember.Type.CreateDefault();

            foreach (var item in modGetterLine.Where(x => x.modCondition != null).Reverse())
            {
                if (result == null)
                {
                    var transformDefault = item.TransformFunc.Type == defaultcase.Type
                        ? defaultcase : item.TransformFunc.Type.CreateDefault();

                    result = Expression.Condition(item.modCondition, item.TransformFunc, transformDefault);
                }
                else
                    result = Expression.Condition(item.modCondition, item.TransformFunc, result);
            }

            return result;
        }


        protected List<Expression> GetMemberMapToTargetAdapter(MemberMapping member, Expression? destination, Expression result, CompileArgument arg)
        {
            var resultLines = new List<Expression>();

            var resultMember = arg.MapType == MapType.MapToTarget || member.UseDestinationValue
                    ? member.DestinationMember.GetExpression(result)
                    : null;

            var modGetterLine = member.GetterLines.All(x => x.Condition == null)
               ? member.GetterLines.Take(1).Select(x => x.ApplyContextSettings(member,arg))
               : member.GetterLines.Select(x => x.ApplyContextSettings(member, arg));

            var single = modGetterLine.GetIfSingle();

            if (single.getterLine != null && single.modCondition == null)

            {
                member.OverrideSettings = single.getterLine.OverrideSettings; // need refactoring

                var adapt = CreateAdaptExpression(single.modgetter, member.DestinationMember.Type, arg, member, resultMember);
                var transformAdapt = adapt.ApplyUseDesitationValueAndInitOnlyProps(member, arg);

                if(adapt == transformAdapt && member.DestinationMember.SetterModifier != AccessModifier.None)
                    resultLines.Add(member.DestinationMember.SetExpression(result, adapt));
                else
                    resultLines.Add(transformAdapt);
            }


            foreach (var item in modGetterLine.Where(x => x.modCondition != null))
            {
                if (member.DestinationMember.SetterModifier == AccessModifier.None
                    && !member.UseDestinationValue)
                    continue;

                member.OverrideSettings = item.getterLine.OverrideSettings; // need refactoring

                var adapt = CreateAdaptExpression(item.modgetter, member.DestinationMember.Type, arg, member, resultMember);
                var transformAdapt = adapt.ApplyUseDesitationValueAndInitOnlyProps(member, arg);

                if (transformAdapt == null)
                    continue;

                if (adapt == transformAdapt && member.DestinationMember.SetterModifier != AccessModifier.None)
                {
                    resultLines.Add(
                    Expression.IfThen(item.modCondition, member.DestinationMember.SetExpression(result, adapt)));
                }
                else
                    resultLines.Add(
                        Expression.IfThen(item.modCondition, transformAdapt));
            }


            if(member.Ignore.Condition != null)
            {
                var test = member.Ignore.IsChildPath
                            ? member.Ignore.Condition.Body
                            : member.Ignore.Condition.Apply(arg.MapType, member.Source, destination);

                var ApplyIgnore = Expression.IfThen(Expression.Not(test), Expression.Block(resultLines));
                resultLines.Clear();
                resultLines.Add(ApplyIgnore);

            }

            return resultLines;
        }

        protected virtual TypeConverterModel CreateTypeConverter(Expression source, Expression? destination, CompileArgument arg)
        {
            Expression customInitExpression = null;
            ConstructorConveterModel? constructorConverter = null;

            var constructUsing = arg.GetConstructUsing();
         
            if (constructUsing != null)
            {
                var args = destination == null ? new[] { source } : new[] { source, destination };
                customInitExpression = constructUsing.Apply(arg.MapType, args)
                    .TrimConversion(true)
                    .To(arg.DestinationType);
            }    
            else
                constructorConverter = DestiantionConstructorSelector(source,arg, out customInitExpression);

            var propertyOrFieldConverter = CreateSourceToDestinationPropertOrFieldConverter(source, arg);


            // RequireDestMemberSource checking section

            var requireDestMemberSource = arg.Settings.RequireDestinationMemberSource;

            if (arg.Context.Config.RequireDestinationMemberSource == true
                && requireDestMemberSource == DestinationMemebers.None)
                requireDestMemberSource = DestinationMemebers.AllMembers;

            if(requireDestMemberSource != DestinationMemebers.None 
                && arg.Settings.SkipDestinationMemberCheck != true)
            {
                var exeptionMessage = new StringBuilder();
                exeptionMessage.AppendLine($"The following members of destination class {arg.DestinationType} do not have a corresponding source member mapped or ignored:");

                var startMessageLength = exeptionMessage.Length;

                if(requireDestMemberSource.HasPropertyOrFields())
                {
                    var unMappingProperties = propertyOrFieldConverter.Members
                        .Where(x => x.IsNotFoundGetter && !x.Ignore.WithOutCondition)
                        .Select(x => x.DestinationMember.Name).ToArray();

                    if(unMappingProperties.Length != 0)
                        exeptionMessage.AppendLine($"- Property or Field members: {string.Join(", ", unMappingProperties)}");
                }

                if(requireDestMemberSource.HasConstrutorMembers())
                {
                    var unMappingCtorParams = constructorConverter == null 
                        ? Array.Empty<string>()
                        : constructorConverter.Members
                            .Where(x => x.IsNotFoundGetter && !x.Ignore.WithOutCondition)
                            .Select(x => x.DestinationMember.Name).ToArray();

                    if(unMappingCtorParams.Length != 0)
                        exeptionMessage.AppendLine($"- Сonstructor parameters: {string.Join(", ", unMappingCtorParams)}");
                }

                if(exeptionMessage.Length > startMessageLength)
                    throw new InvalidOperationException(exeptionMessage.ToString());
            }


            return new TypeConverterModel() { ConstructorMapping = constructorConverter, PropertyOrFieldMapping = propertyOrFieldConverter, CustomInitExpression = customInitExpression };
        }

        protected ConstructorConveterModel? OptimalConstuctorSelector(IEnumerable<ConstructorInfo> constructors, Expression source, CompileArgument arg)
        {
            var MappingScoreGroups = constructors
                .Select(it => CreateSourceToDestinationConstructorConveter(source, it, arg))
                // if param of ctor is optional it case equal find getter state
                .GroupBy(x => x.Members.Where(y => !y.IsNotFoundGetter || !y.DestinationMember.IsRequired).Count() - (x.ConstructorInfo?.GetParameters().Count()).GetValueOrDefault())
                .OrderByDescending(x => x.Key)
                .Take(2);

            return MappingScoreGroups.FirstOrDefault()
                ?.OrderByDescending(x => x.ConstructorInfo?.GetParameters().Count())
                .FirstOrDefault();
        }

        protected virtual ConstructorConveterModel? DestiantionConstructorSelector(Expression source, CompileArgument arg, out Expression? defaultExpression)
        {
            ConstructorConveterModel? constructorMapping = null;
            defaultExpression = null;

            if (arg.Settings.MapToConstructor is ConstructorInfo customCtor)
                constructorMapping = CreateSourceToDestinationConstructorConveter(source, customCtor, arg);
            else
            {
                var destType = arg.DestinationType.IsInterface
                  ? DynamicTypeGenerator.GetTypeForInterface(arg.DestinationType, arg.Settings.Includes.Count > 0) ?? arg.DestinationType
                  : arg.DestinationType;


                ConstructorInfo[] destypeConstuctors = arg.Settings.EnableNonPublicConstructors.GetValueOrDefault()
                    ? destType!.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    : destType!.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

                if(destType.HasDefaultConstructor(destypeConstuctors,out var constructor))
                {
                    if(constructor is not null)
                        constructorMapping = CreateSourceToDestinationConstructorConveter(source, constructor, arg);
                    
                    // ValueType
                    if (constructor is null && destypeConstuctors.Length == 0) 
                        defaultExpression = Expression.New(arg.DestinationType);
                    else
                        constructorMapping = OptimalConstuctorSelector(destypeConstuctors, source, arg);
                }
                else
                    constructorMapping = OptimalConstuctorSelector(destypeConstuctors, source, arg);

            }

            return constructorMapping;
        }

        protected override Expression CreateExpressionBody(Expression source, Expression? destination, CompileArgument arg)
        {
            arg.TypeConverter = CreateTypeConverter(source,destination,arg);

            return base.CreateExpressionBody(source, destination, arg);
        }

        #endregion
    }
}
