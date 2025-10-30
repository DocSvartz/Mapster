using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenRegisteringAndMappingRace
    {
        [TestCleanup]
        public void TestCleanup()
        {
            TypeAdapterConfigFactory.GlobalSettings.Clear();
            TypeAdapterConfigFactory.GlobalSettings.RequireExplicitMapping = false;
            TypeAdapterConfigFactory.GlobalSettings.RequireDestinationMemberSource = false;
            if(TypeAdapterConfigFactory.GlobalSettings is IConfigConcurrency config)
                config.ConcurrencyEnvironment = false;
        }


        [TestMethod]
        public void Types_Map_Successfully_If_Mapping_Applied_First()
        {
            TypeAdapterConfigFactory.GlobalSettings.RequireDestinationMemberSource = true;

            var simplePoco = new WhenAddingCustomMappings.SimplePoco {Id = Guid.NewGuid(), Name = "TestName"};

            TypeAdapterConfig<WhenAddingCustomMappings.SimplePoco, WeirdPoco>.NewConfig()
                .Map(dest => dest.IHaveADifferentId, src => src.Id)
                .Map(dest => dest.MyNamePropertyIsDifferent, src => src.Name)
                .Ignore(dest => dest.Children);

            TypeAdapter.Adapt<WhenAddingCustomMappings.SimplePoco, WeirdPoco>(simplePoco);
        }

        [TestMethod, TestCategory("speed"), Ignore]
        public void Race_Condition_Produces_Error()
        {
            TypeAdapterConfigFactory.GlobalSettings.RequireDestinationMemberSource = true;

            var simplePoco = new WhenAddingCustomMappings.SimplePoco {Id = Guid.NewGuid(), Name = "TestName"};

            var exception = Should.Throw<AggregateException>(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    Parallel.Invoke(
                        () =>
                        {
                            TypeAdapterConfig<WhenAddingCustomMappings.SimplePoco, WeirdPoco>.NewConfig()
                                .Map(dest => dest.IHaveADifferentId, src => src.Id)
                                .Map(dest => dest.MyNamePropertyIsDifferent, src => src.Name)
                                .Ignore(dest => dest.Children);
                        },
                        () => { TypeAdapter.Adapt<WeirdPoco>(simplePoco); }
                        );
                }
            });

            exception.InnerException.ShouldBeOfType(typeof(CompileException));

        }

        [TestMethod, TestCategory("speed"), Ignore]
        public void Explicit_Mapping_Requirement_Throws_Before_Mapping_Attempted()
        {
            TypeAdapterConfigFactory.GlobalSettings.RequireExplicitMapping = true;
            TypeAdapterConfigFactory.GlobalSettings.RequireDestinationMemberSource = true;

            var simplePoco = new WhenAddingCustomMappings.SimplePoco { Id = Guid.NewGuid(), Name = "TestName" };

            Should.Throw<AggregateException>(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                   Parallel.Invoke(
                        () =>
                        {
                            TypeAdapterConfig<WhenAddingCustomMappings.SimplePoco, WeirdPoco>.NewConfig()
                                .Map(dest => dest.IHaveADifferentId, src => src.Id)
                                .Map(dest => dest.MyNamePropertyIsDifferent, src => src.Name)
                                .Ignore(dest => dest.Children);
                        },
                        () => { TypeAdapter.Adapt<WeirdPoco>(simplePoco); }
                        );
                }
            });

            //Type should map at the end because mapping has completed.
            TypeAdapter.Adapt<WhenAddingCustomMappings.SimplePoco, WeirdPoco>(simplePoco);
        }


        [TestMethod]
        public void Explicit_Mapping_Requirementd()
        {
            
            TypeAdapterConfigFactory.GlobalSettings.RequireExplicitMapping = true;
            TypeAdapterConfigFactory.GlobalSettings.RequireDestinationMemberSource = true;

            var simplePoco = new WhenAddingCustomMappings.SimplePoco { Id = Guid.NewGuid(), Name = "TestName" };

            TypeAdapterConfigConcurrency<WhenAddingCustomMappings.SimplePoco, WeirdPoco>.NewConfig()
                
                                 .Map(dest => dest.IHaveADifferentId, src => src.Id)
                                 .Map(dest => dest.MyNamePropertyIsDifferent, src => src.Name)
                                 .Ignore(dest => dest.Children)
                                 .FinalizeConfig()
                                 ;

            
                for (int i = 0; i < 100; i++)
                {
                    Parallel.Invoke(
                         () =>
                         {
                             TypeAdapterConfigConcurrency<WhenAddingCustomMappings.SimplePoco, WeirdPoco>.NewConfig()
                                 .Map(dest => dest.IHaveADifferentId, src => src.Id)
                                 .Map(dest => dest.MyNamePropertyIsDifferent, src => src.Name)
                                 .Ignore(dest => dest.Children)
                                 .FinalizeConfig()
                                 ;

                         },
                         () => 
                         { 
                             TypeAdapter.Adapt<WeirdPoco>(simplePoco); }
                         );
                }
        }

        [TestMethod]
        public void Scan_Explicit_Mapping_Requirementd()
        {

            TypeAdapterConfigFactory.GlobalSettings.RequireExplicitMapping = true;
            TypeAdapterConfigFactory.GlobalSettings.RequireDestinationMemberSource = true;

            var simplePoco = new WhenAddingCustomMappings.SimplePoco { Id = Guid.NewGuid(), Name = "TestName" };

            TypeAdapterConfigFactory.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            //TypeAdapter.Adapt<WhenAddingCustomMappings.SimplePoco, WeirdPoco>(simplePoco);

            for (int i = 0; i < 100; i++)
            {
                Parallel.Invoke(
                     () =>
                     {
                         TypeAdapterConfigFactory.GlobalSettings.ScanConcurrency(Assembly.GetExecutingAssembly());

                     },
                     () =>
                     {
                         TypeAdapter.Adapt<WeirdPoco>(simplePoco);
                     }
                     );
            }
        }

    }


    #region TestClasses

    public class RegData : IRegister
    {
        public void Register(ITypeAdapterConfig config)
        {
            config.NewConfig<WhenAddingCustomMappings.SimplePoco, WeirdPoco>()
            .Map(dest => dest.IHaveADifferentId, src => src.Id)
            .Map(dest => dest.MyNamePropertyIsDifferent, src => src.Name)
            .Ignore(dest => dest.Children);
            
        }
    }


    public class SimplePoco
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ChildPoco
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ChildDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string UnmappedChildMember { get; set; }
    }


    public class WeirdPoco
    {
        public Guid IHaveADifferentId { get; set; }

        public string MyNamePropertyIsDifferent { get; set; }

        public List<ChildDto> Children { get; set; }
    }

    #endregion




}