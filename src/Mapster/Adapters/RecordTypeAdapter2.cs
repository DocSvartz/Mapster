using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Mapster.IgnoreDictionary;

namespace Mapster.Adapters
{
    internal class RecordTypeAdapter2 : ClassAdapter
    {
        protected override int Score => -149;
        protected override bool UseTargetValue => false;
        protected override bool CanMap(PreCompileArgument arg)
        {
            return arg.DestinationType.IsRecordType();
        }

        protected override bool CanInline(Expression source, Expression? destination, CompileArgument arg)
        {
            if (arg.MapType == MapType.Projection)
                return true;
            return false;
        }

        protected override Expression CreateInlineExpression(Expression source, CompileArgument arg, bool IsRequiredOnly = false)
        {
            return CreateInstantiationExpression(source, arg);
        }

        protected override Expression CreateInstantiationExpression(Expression source, Expression? destination, CompileArgument arg)
        {
            //new TDestination(src.Prop1, src.Prop2)
            Expression installExpr;

            if (arg.GetConstructUsing() != null || arg.Settings.MapToConstructor != null || arg.DestinationType == null)
                installExpr = base.CreateInstantiationExpression(source, destination, arg);
            else
            {
                var ctor = arg.DestinationType.GetConstructors()
                        .OrderByDescending(it => it.GetParameters().Length).ToArray().FirstOrDefault(); // Will be used public constructor with the maximum number of parameters 
                var classModel = GetConstructorModel(ctor, false);
                var restorParamModel = GetSetterModel(arg);
                var classConverter = CreateClassConverter(source, classModel, arg, ctorMapping: true);
                installExpr = CreateInstantiationExpression(source, classConverter, arg, destination, restorParamModel);
            }


            return RecordInlineExpression(source, destination, arg, installExpr); // Activator field when not include in public ctor
        }

        private Expression? RecordInlineExpression(Expression source, Expression? destination, CompileArgument arg, Expression installExpr)
        {
            //new TDestination {
            //  Prop1 = convert(src.Prop1),
            //  Prop2 = convert(src.Prop2),
            //}

            var exp = installExpr;
            var memberInit = exp as MemberInitExpression;
            var newInstance = memberInit?.NewExpression ?? (NewExpression)exp;
            var contructorMembers = newInstance.Constructor?.GetParameters().ToList() ?? new();
            var classModel = GetSetterModel(arg);
            var classConverter = CreateClassConverter(source, classModel, arg, destination: destination, recordRestorMemberModel: classModel);
            var members = classConverter.Members;

           

            var lines = new List<MemberBinding>();
            if (memberInit != null)
                lines.AddRange(memberInit.Bindings);
            foreach (var member in members)
            {
                if (!arg.Settings.Resolvers.Any(r => r.DestinationMemberName == member.DestinationMember.Name)
                    && contructorMembers.Any(x => string.Equals(x.Name, member.DestinationMember.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                if (member.DestinationMember.SetterModifier == AccessModifier.None)
                    continue;

                var adapt = CreateAdaptExpression(member.Getter, member.DestinationMember.Type, arg, member);

                if (arg.MapType != MapType.MapToTarget)
                {
                    if (member.Ignore.Condition != null || arg.Settings.IgnoreNullValues.GetValueOrDefault())
                        continue;

                    //special null property check for projection
                    //if we don't set null to property, EF will create empty object
                    //except collection type & complex type which cannot be null
                    if (arg.MapType == MapType.Projection
                        && member.Getter.Type != member.DestinationMember.Type
                        && !member.Getter.Type.IsCollection()
                        && !member.DestinationMember.Type.IsCollection()
                        && member.Getter.Type.GetTypeInfo().GetCustomAttributesData().All(attr => attr.GetAttributeType().Name != "ComplexTypeAttribute"))
                    {
                        adapt = member.Getter.NotNullReturn(adapt);
                    }
                    var bind = Expression.Bind((MemberInfo)member.DestinationMember.Info!, adapt);
                    lines.Add(bind);

                }
                if (arg.MapType == MapType.MapToTarget)
                {
                    var bind = Expression.Bind((MemberInfo)member.DestinationMember.Info!, MapToTargetRestorePropertyOrField(destination,member));
                    lines.Add(bind);
                }
            }


            if (arg.MapType == MapType.MapToTarget)
                lines.AddRange(RecordIngnoredWithoutConditonRestore(destination, arg, contructorMembers, classModel));

            return Expression.MemberInit(newInstance, lines);
        }

        private List<MemberBinding> RecordIngnoredWithoutConditonRestore(Expression? destination, CompileArgument arg, List<ParameterInfo> contructorMembers, ClassModel restorPropertyModel)
        {
            var members = restorPropertyModel.Members
                             .Where(x => arg.Settings.Ignore.Any(y => y.Key == x.Name));

            var lines = new List<MemberBinding>();


            foreach (var member in members)
            {
                if (destination == null)
                    continue;

                IgnoreItem ignore;
                ProcessIgnores(arg, member, out ignore);

                if (member.SetterModifier == AccessModifier.None ||
                   ignore.Condition != null ||
                   contructorMembers.Any(x => string.Equals(x.Name, member.Name, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                lines.Add(Expression.Bind((MemberInfo)member.Info, Expression.MakeMemberAccess(destination, (MemberInfo)member.Info)));
            }

            return lines;
        }

       



        protected Expression MapToTargetRestorePropertyOrField(Expression? destination, MemberMapping member)
        {
            var compareNull = Expression.Equal(destination, Expression.Constant(null, destination.Type));
            return Expression.Condition(compareNull, member.DestinationMember.Type.CreateDefault(), member.DestinationMember.GetExpression(destination));
        }

        protected override Expression CreateBlockExpression(Expression source, Expression result, Expression? destination, CompileArgument arg)
        {
            //### !IgnoreNullValues
            //dest.Prop1 = convert(src.Prop1);
            //dest.Prop2 = convert(src.Prop2);

            //### IgnoreNullValues
            //if (src.Prop1 != null)
            //  dest.Prop1 = convert(src.Prop1);
            //if (src.Prop2 != null)
            //  dest.Prop2 = convert(src.Prop2);

            var classModel = GetSetterModel(arg);
            var classConverter = CreateClassConverter(source, classModel, arg, result);
            var members = classConverter.Members;

            var lines = new List<Expression>();
            Dictionary<LambdaExpression, Tuple<List<Expression>, Expression>>? conditions = null;
            foreach (var member in members)
            {
               
                if (arg.Settings.IgnoreNullValues == false)
                {
                    if (member.UseDestinationValue || member.Ignore.Condition != null) ;
                    else
                        continue;
                }
               
               

                var destMember = arg.MapType == MapType.MapToTarget || member.UseDestinationValue
                    ? member.DestinationMember.GetExpression(result)
                    : null;

                var adapt = CreateAdaptExpression(member.Getter, member.DestinationMember.Type, arg, member, destMember);

                if (member.UseDestinationValue
                    && member.DestinationMember.Type.IsMapsterImmutable()
                    && member.DestinationMember.SetterModifier == AccessModifier.None)
                {

                    if (arg.Settings.IgnoreNullValues == true && member.Getter.CanBeNull())
                    {
                        var condition = Expression.NotEqual(member.Getter, Expression.Constant(null, member.Getter.Type));

                        if (arg.MapType == MapType.MapToTarget)
                        {
                            adapt = Expression.Condition(condition, adapt, MapToTargetRestorePropertyOrField(destination, member));
                            adapt = SetValueTypeAutoPropertyByReflection(member, adapt, classModel);
                        }
                        else
                            adapt = Expression.IfThen(condition, SetValueTypeAutoPropertyByReflection(member, adapt, classModel));
                    }
                    else
                    {
                        if (member.DestinationMember is PropertyModel && arg.MapType != MapType.Projection)
                            adapt = SetValueTypeAutoPropertyByReflection(member, adapt, classModel);
                        else
                            continue;
                    }

                    if (adapt == Expression.Empty())
                        continue;
                }


                if (!member.UseDestinationValue)
                {
                    if (arg.Settings.IgnoreNullValues == true && member.Getter.CanBeNull()
                        && member.DestinationMember.SetterModifier != AccessModifier.None)
                    {
                        if (adapt is ConditionalExpression condEx)
                        {
                            if (condEx.Test is BinaryExpression { NodeType: ExpressionType.Equal } binEx &&
                                binEx.Left == member.Getter &&
                                binEx.Right is ConstantExpression { Value: null })
                                adapt = condEx.IfFalse;

                            if (arg.MapType == MapType.MapToTarget)
                                adapt = Expression.Condition(condEx.Test, MapToTargetRestorePropertyOrField(destination, member), adapt);
                        }
                        adapt = member.DestinationMember.SetExpression(result, adapt);
                        var condition = Expression.NotEqual(member.Getter, Expression.Constant(null, member.Getter.Type));
                        adapt = Expression.IfThen(condition, adapt);
                    }
                    else
                    {
                        //Todo Try catch block should be removed after pull request approved
                        try
                        {
                            if (member.DestinationMember.SetterModifier != AccessModifier.None)
                            {
                                var destinationPropertyInfo = (PropertyInfo)member.DestinationMember.Info!;
                                adapt = destinationPropertyInfo.IsInitOnly()
                                    ? SetValueByReflection(member, (MemberExpression)adapt)
                                    : member.DestinationMember.SetExpression(result, adapt);
                            }

                        }
                        catch (Exception e)
                        {
                            adapt = member.DestinationMember.SetExpression(result, adapt);
                        }
                    }
                }
                else if (!adapt.IsComplex())
                    continue;

                if (member.Ignore.Condition != null)
                {
                    conditions ??= new Dictionary<LambdaExpression, Tuple<List<Expression>, Expression>>();
                    if (!conditions.TryGetValue(member.Ignore.Condition, out var tuple))
                    {
                        var body = member.Ignore.IsChildPath
                            ? member.Ignore.Condition.Body
                            : member.Ignore.Condition.Apply(arg.MapType, source, result);
                        tuple = Tuple.Create(new List<Expression>(), body);
                        conditions[member.Ignore.Condition] = tuple;
                    }

                    tuple.Item1.Add(adapt);
                }
                else
                    lines.Add(adapt);
            }

            if (conditions != null)
            {
                foreach (var kvp in conditions)
                {
                    var condition = Expression.IfThen(
                        ExpressionEx.Not(kvp.Value.Item2),
                        Expression.Block(kvp.Value.Item1));
                    lines.Add(condition);
                }
            }

            return lines.Count > 0 ? (Expression)Expression.Block(lines) : Expression.Empty();
        }
    }
}
