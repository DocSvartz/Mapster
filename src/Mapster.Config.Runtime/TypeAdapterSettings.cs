// ReSharper disable ArrangeAccessorOwnerBody

namespace Mapster.Config.Runtime
{
    public class TypeAdapterSettings : TypeAdapterSettingsBase, ISettingCloneble<TypeAdapterSettings>
    {
       
        //public IgnoreDictionary Ignore
        //{
        //    get => Get(nameof(Ignore), () => new IgnoreDictionary());
        //}
        //public List<DestinationTransform> DestinationTransforms
        //{
        //    get => Get(nameof(DestinationTransforms), () => new List<DestinationTransform>());
        //}
       
        //public ProjectToTypeAutoMapping ProjectToTypeMapConfig
        //{
        //    get => GetEnum(nameof(ProjectToTypeMapConfig), ()=> default(ProjectToTypeAutoMapping));
        //    set => Set(nameof(ProjectToTypeMapConfig), value);
        //}
        //public List<Func<IMemberModel, MemberSide, bool?>> ShouldMapMember
        //{
        //    get => Get(nameof(ShouldMapMember), () => new List<Func<IMemberModel, MemberSide, bool?>>());
        //}
        //public List<Func<Expression, IMemberModel, CompileArgument, Expression?>> ValueAccessingStrategies
        //{
        //    get => Get(nameof(ValueAccessingStrategies), () => new List<Func<Expression, IMemberModel, CompileArgument, Expression?>>());
        //}
        //public List<InvokerModel> Resolvers
        //{
        //    get => Get(nameof(Resolvers), () => new List<InvokerModel>());
        //}
        //public List<Func<CompileArgument, LambdaExpression>> BeforeMappingFactories
        //{
        //    get => Get(nameof(BeforeMappingFactories), () => new List<Func<CompileArgument, LambdaExpression>>());
        //}
        //public List<Func<CompileArgument, LambdaExpression>> AfterMappingFactories
        //{
        //    get => Get(nameof(AfterMappingFactories), () => new List<Func<CompileArgument, LambdaExpression>>());
        //}
        //public List<TypeTuple> Includes
        //{
        //    get => Get(nameof(Includes), () => new List<TypeTuple>());
        //}
        //public List<Func<IMemberModel, MemberSide, string?>> GetMemberNames
        //{
        //    get => Get(nameof(GetMemberNames), () => new List<Func<IMemberModel, MemberSide, string?>>());
        //}
        //public List<Func<IMemberModel, bool>> UseDestinationValues
        //{
        //    get => Get(nameof(UseDestinationValues), () => new List<Func<IMemberModel, bool>>());
        //}
        //public Func<CompileArgument, LambdaExpression>? ConstructUsingFactory
        //{
        //    get => Get<Func<CompileArgument, LambdaExpression>>(nameof(ConstructUsingFactory));
        //    set => Set(nameof(ConstructUsingFactory), value);
        //}
        //public Func<CompileArgument, LambdaExpression>? ConverterFactory
        //{
        //    get => Get<Func<CompileArgument, LambdaExpression>>(nameof(ConverterFactory));
        //    set => Set(nameof(ConverterFactory), value);
        //}
        //public Func<CompileArgument, LambdaExpression>? ConverterToTargetFactory
        //{
        //    get => Get<Func<CompileArgument, LambdaExpression>>(nameof(ConverterToTargetFactory));
        //    set => Set(nameof(ConverterToTargetFactory), value);
        //}
       
        //public Action<TypeAdapterConfig>? Fork
        //{
        //    get => Get<Action<TypeAdapterConfig>>(nameof(Fork));
        //    set => Set(nameof(Fork), value);
        //}

        public bool Compiled { get; set; }

        public TypeAdapterSettings Clone()
        {
            var settings = new TypeAdapterSettings();
            settings.Apply(this);
            return settings;
        }
    }
}