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
                    var bind = Expression.Bind((MemberInfo)member.DestinationMember.Info!, adapt);
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
    }
}
