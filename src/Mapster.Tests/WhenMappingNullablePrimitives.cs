using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingNullablePrimitives
    {

        [TestMethod]
        public void Can_Map_From_Null_Source_To_Non_Nullable_Existing_Target()
        {
            TypeAdapterConfig<NullablePrimitivesPoco, NonNullablePrimitivesDto>.Clear();

            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName" };

            var dto = new NonNullablePrimitivesDto();

            TypeAdapter.Adapt(poco, dto);

            dto = TypeAdapter.Adapt<NullablePrimitivesPoco, NonNullablePrimitivesDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeFalse();
        }

        [TestMethod]
        public void Can_Map_From_Null_Source_To_Non_Nullable_Target()
        {
            TypeAdapterConfig<NullablePrimitivesPoco, NonNullablePrimitivesDto>.Clear();

            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName" };

            NonNullablePrimitivesDto dto = TypeAdapter.Adapt<NullablePrimitivesPoco, NonNullablePrimitivesDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeFalse();
        }

        [TestMethod]
        public void Can_Map_From_Nullable_Source_To_Nullable_Target()
        {
            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName" };

            NullablePrimitivesPoco2 dto = TypeAdapter.Adapt<NullablePrimitivesPoco, NullablePrimitivesPoco2>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeNull();
            dto.Amount.ShouldBeNull();
        }

        [TestMethod]
        public void Can_Map_From_Nullable_Source_To_Nullable_Existing_Target()
        {
            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName" };

            NullablePrimitivesPoco2 dto = new NullablePrimitivesPoco2
            {
                IsImport = true,
                Amount = 1,
            };

            TypeAdapter.Adapt(poco, dto);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeNull();
            dto.Amount.ShouldBeNull();
        }

        [TestMethod]
        public void Can_Map_From_Nullable_Source_With_Values_To_Non_Nullable_Target()
        {
            TypeAdapterConfig<NullablePrimitivesPoco, NonNullablePrimitivesDto>.NewConfig()
                .Compile();
            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName", IsImport = true, Amount = 10};

            NonNullablePrimitivesDto dto = TypeAdapter.Adapt<NullablePrimitivesPoco, NonNullablePrimitivesDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeTrue();
            dto.Amount.ShouldBe(10);
        }

        [TestMethod]
        public void Can_Map_From_Nullable_Source_Without_Values_To_Non_Nullable_Target()
        {
            TypeAdapterConfig<NullablePrimitivesPoco, NonNullablePrimitivesDto>.NewConfig()
                .Compile();
            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName"};

            NonNullablePrimitivesDto dto = TypeAdapter.Adapt<NullablePrimitivesPoco, NonNullablePrimitivesDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeFalse();
            dto.Amount.ShouldBe(0);
        }

        [TestMethod]
        public void Can_Map_From_Nullable_Source_With_Values_To_Explicitly_Mapped_Non_Nullable_Target()
        {
            TypeAdapterConfig<NullablePrimitivesPoco, NonNullablePrimitivesDto>.NewConfig()
                .Map(dest => dest.Amount, src => src.Amount)
                .Map(dest => dest.IsImport, src => src.IsImport)
                .Map(dest => dest.MyFee, src => src.Fee)
                .Compile();

            var poco = new NullablePrimitivesPoco { Id = Guid.NewGuid(), Name = "TestName", Fee = 20, IsImport = true, Amount = 10};

            NonNullablePrimitivesDto dto = TypeAdapter.Adapt<NullablePrimitivesPoco, NonNullablePrimitivesDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.IsImport.ShouldBeTrue();
            dto.Amount.ShouldBe(10);
            dto.MyFee.ShouldBe(20);
        }

        [TestMethod]
        public void Can_Map_From_Non_Nullable_Source_To_Nullable_Target()
        {
            var dto = new NonNullablePrimitivesDto { Id = Guid.NewGuid(), Name = "TestName", IsImport = true};

            NullablePrimitivesPoco poco = TypeAdapter.Adapt<NonNullablePrimitivesDto, NullablePrimitivesPoco>(dto);

            poco.Id.ShouldBe(dto.Id);
            poco.Name.ShouldBe(dto.Name);
            poco.IsImport.GetValueOrDefault().ShouldBeTrue();
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/414
        /// </summary>
        [TestMethod]
        public void MappingNullTuple()
        {
            TypeAdapterConfig<(string?, string?, Application414), Output414>.NewConfig()
                .Map(dest => dest, src => src.Item1)
                .Map(dest => dest, src => src.Item2)
                .Map(dest => dest.Application, src => src.Item3 == null ? (Application414)null : new Application414()
                 {
                     Id = src.Item3.Id,
                     Name = src.Item3.Name
                 });

            (string, string, Application414) source = (null, null, null);

            var result = source.Adapt<Output414>();

            result.Item1.ShouldBeNull();
            result.Item2.ShouldBeNull();
            result.Application.ShouldBeNull();
        }

        #region TestClasses


        public class Output414
        {
            public string Item1 { get; set; }

            public string Item2 { get; set; }

            public Application414 Application { get; set; }
        }

        public class Application414
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }

        public class NullablePrimitivesPoco
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public bool? IsImport { get; set; }

            public decimal? Amount { get; set; }

            public decimal? Fee { get; set; }
        }

        public class NullablePrimitivesPoco2
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public bool? IsImport { get; set; }

            public decimal? Amount { get; set; }

            public decimal? Fee { get; set; }
        }

        public class NonNullablePrimitivesDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public bool IsImport { get; set; }

            public decimal Amount { get; set; }

            public decimal MyFee { get; set; }
        }

        #endregion 
    }
}