using Mapster.EFCore.Tests.Models;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapster.EFCore.Tests
{
    [TestClass]
    public class EFCoreTest
    {
        [TestMethod]
        public void TestFindObject()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new SchoolContext(options);
            DbInitializer.Initialize(context);

            var dto = new StudentDto
            {
                ID = 7,
                Enrollments = new List<EnrollmentItemDto>
                {
                    new EnrollmentItemDto
                    {
                        EnrollmentID = 12,
                        Grade = Grade.F,
                    }
                }
            };
            var poco = context.Students.Include(it => it.Enrollments)
                .First(it => it.ID == dto.ID);

            dto.BuildAdapter()
                .EntityFromContext(context)
                .AdaptTo(poco);

            var first = poco.Enrollments.First();
            first.CourseID.ShouldBe(3141);
            first.Grade.ShouldBe(Grade.F);
        }

        [TestMethod]
        public async Task TestFindSingleObjectUsingProjectToType()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new SchoolContext(options);
            DbInitializer.Initialize(context);

            var mapsterInstance = new Mapper();

            var query = context.Students.Where(s => s.ID == 1);

            async Task<StudentDto> FirstExecute() =>
                await mapsterInstance.From(query)
                    .ProjectToType<StudentDto>()
                    .FirstOrDefaultAsync();

            await Should.NotThrowAsync(async () =>
            {
                var first = await FirstExecute();

                first.ShouldNotBeNull();
                first.ID.ShouldBe(1);
                first.LastName.ShouldBe("Alexander");
            });
        }

        [TestMethod]
        public void MapperInstance_From_OrderBy()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new SchoolContext(options);
            DbInitializer.Initialize(context);

            var mapsterInstance = new Mapper();

            var query = context.Students.Include(it => it.Enrollments);
            var orderedQuery = mapsterInstance.From(query)
                .ProjectToType<StudentDto>()
                .OrderBy(s => s.LastName);

            var first = orderedQuery.First();
            first.LastName.ShouldBe("Alexander");

            var last = orderedQuery.Last();
            last.LastName.ShouldBe("Olivetto");
        }

        [TestMethod]
        public void MergeIncludeWhenUsingEFCoreProjectToType()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new SchoolContext(options);
            DbInitializer.Initialize(context);

            var mapsterInstance = new Mapper();

            var query = context.Students
                .Include(x => x.Enrollments.OrderByDescending(x => x.StudentID).Take(1))
                .EFCoreProjectToType<StudentDto>();

            var first = query.First();

            first.Enrollments.Count.ShouldBe(1);
            first.LastName.ShouldBe("Alexander");
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/875
        /// </summary>
        [TestMethod]
        public void NotMemberNameEFCoreProjectToType_not_Error()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new SchoolContext(options);
            DbInitializer.Initialize(context);

            var config = new TypeAdapterConfig();

                config
                    .NewConfig<Student, StudentDto>()
                    .Map(dest => dest.LastName, src => src.GetLastName());
   
            Should.NotThrow(() =>
            {
                var query = context.Students
               .Include(x => x.Enrollments.OrderByDescending(x => x.StudentID).Take(1))
               .EFCoreProjectToType<StudentDto>(config);
            });

        }


        [TestMethod]
        public void RecordsEFCoreProjectToType_not_Error()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new SchoolContext(options);
            DbInitializer.Initialize(context);


            Should.NotThrow(() =>
            {
                var query = context.Students
                .Include(x => x.Enrollments.OrderByDescending(x => x.StudentID).Take(1))
                .EFCoreProjectToType<StudentRecordDto>();

                var first = query.First();

                first.Enrollments.Count.ShouldBe(1);
                first.LastName.ShouldBe("Alexander");

            });

        }




    }


    public record StudentRecordDto
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public ICollection<EnrollmentItemDto> Enrollments { get; set; }
    }

    public class StudentDto
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public ICollection<EnrollmentItemDto> Enrollments { get; set; }
    }

    public class EnrollmentItemDto
    {
        public int EnrollmentID { get; set; }
        public Grade? Grade { get; set; }
    }
}
