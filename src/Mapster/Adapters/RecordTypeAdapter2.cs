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
            Expression installExpr = arg.TypeConverter.CustomInitExpression ?? CreateInstantiationExpression(source, arg.TypeConverter.ConstructorMapping!, arg);

            if (arg.TypeConverter.IsNewOrMemberInit == false)
                return installExpr;

            if (arg.MapType != MapType.MapToTarget)
                return RecordInlineExpression(source, arg, installExpr);
            else
                return RecordInlineMapToTargetExpression(source, arg, installExpr);
        }

        private Expression RecordInlineExpression(Expression source, CompileArgument arg, Expression installExpr)
        {
            //new TDestination {
            //  Prop1 = convert(src.Prop1),
            //  Prop2 = convert(src.Prop2),
            //}

            var exp = installExpr;
            var memberInit = installExpr as MemberInitExpression;
            var newInstance = memberInit?.NewExpression ?? (NewExpression)exp;
            var members = arg.TypeConverter.PropertyOrFieldMapping.Members;

            var lines = new List<MemberBinding>(arg.TypeConverter.GetInitExpMemberBindings);

            foreach (var member in members)
            {
                if (arg.TypeConverter.IsUsingInInit(member.DestinationMember.Name) && !member.IsMaybeReMapping)
                    continue;

                if (member.DestinationMember.SetterModifier == AccessModifier.None)
                    continue;
               
                if (member.Ignore.WithOutCondition || member.IsNotFoundGetter)
                    continue;

                var adapt = GetMemberInlineAdapter(member, arg);

                var bind = Expression.Bind((MemberInfo)member.DestinationMember.Info!, adapt);
                lines.Add(bind);
            }

            return Expression.MemberInit(newInstance, lines);
        }

        private Expression RecordInlineMapToTargetExpression(Expression source, CompileArgument arg, Expression installExpr)
        {
            var exp = installExpr;
            var memberInit = installExpr as MemberInitExpression;
            var newInstance = memberInit?.NewExpression ?? (NewExpression)exp;
            var members = arg.TypeConverter.PropertyOrFieldMapping.Members;

            var lines = new List<MemberBinding>(arg.TypeConverter.GetInitExpMemberBindings);

            foreach (var member in members)
            {
                if (arg.TypeConverter.IsUsingInInit(member.DestinationMember.Name))
                {
                    var removeMember = lines.FirstOrDefault(x => x.Member.Name == member.DestinationMember.Name);
                    if(removeMember != null)
                        lines.Remove(removeMember);
                }

                if (member.DestinationMember.SetterModifier == AccessModifier.None)
                    continue;

                Expression? adapt = null;

                if(member.Ignore.WithOutCondition || member.IsNotFoundGetter)
                {
                    if(member.RestoreDestinationMemberExp != null)
                        adapt = member.RestoreDestinationMemberExp;
                }
                else
                    adapt = GetMemberInlineAdapter(member, arg);

                if (adapt is null)
                    continue;

                var bind = Expression.Bind((MemberInfo)member.DestinationMember.Info!, adapt);
                lines.Add(bind);
            }

            return Expression.MemberInit(newInstance, lines);
        }


        protected override Expression CreateBlockExpression(Expression source, Expression result, Expression? destination, CompileArgument arg)
        {
            //var classModel = GetSetterModel(arg);
            //var classConverter = CreateSourceToDestinationConveter(source, classModel, arg);
            var members = arg.TypeConverter.PropertyOrFieldMapping.Members;

            var lines = new List<Expression>();

            foreach (var member in members)
            {
                if (member.UseDestinationValue && member.DestinationMember.SetterModifier == AccessModifier.None)
                {
                    if (arg.MapType == MapType.MapToTarget && member.RestoreDestinationMemberExp != null)
                    {
                        if (member.DestinationMember.IsBackField) // not restore Backfields
                            continue;

                        var adapt = member.RestoreDestinationMemberExp
                            .ApplyUseDesitationValueAndInitOnlyProps(member, arg);

                        if (adapt == null)
                            continue;

                        lines.Add(adapt);
                    }
                    if (member.Ignore.WithOutCondition && !member.IsMaybeReMapping)
                        continue;
                        
                    lines.AddRange(GetMemberBlockAdapter(member, destination, result, arg));
                }
                else if(arg.TypeConverter.IsNewOrMemberInit == false 
                    && member.DestinationMember.SetterModifier != AccessModifier.None)
                {
                    if(arg.MapType == MapType.MapToTarget)
                    {
                        if (member.Ignore.WithOutCondition && !member.IsMaybeReMapping)
                        {
                            if (member.RestoreDestinationMemberExp != null)
                                lines.Add(
                                    member.DestinationMember.SetExpression(arg.Result, member.RestoreDestinationMemberExp));
                        }
                        else
                            lines.AddRange(GetMemberBlockAdapter(member, destination, result, arg));
                    }
                    else
                    {
                        if (member.Ignore.WithOutCondition && !member.IsMaybeReMapping)
                            continue;
                        lines.AddRange(GetMemberBlockAdapter(member, destination, result, arg));
                    }
                }
            }

            return lines.Count > 0 ? (Expression)Expression.Block(lines) : Expression.Empty();
        }

        protected override Expression GetMemberInlineAdapter(MemberMapping member, CompileArgument arg, Expression? defaultValue = null)
        {
            return base.GetMemberInlineAdapter(member, arg, TryRestoreRecordMember(member,arg));
        }
    }
}
