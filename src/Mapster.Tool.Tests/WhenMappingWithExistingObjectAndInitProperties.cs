using System.Reflection;
using FluentAssertions;
using Mapster.Tool.Tests.Mappers;

namespace Mapster.Tool.Tests;

/// <summary>
/// Tests for https://github.com/MapsterMapper/Mapster/issues/536
/// </summary>
public class WhenMappingWithExistingObjectAndInitProperties : TestBase
{
    [Fact]
    public void MapWithReflection()
    {
        TypeAdapterConfig.GlobalSettings
            .Scan(Assembly.GetExecutingAssembly());
        
        var userMapper = GetMappingInterface<IUserMapper>();
        var expected = "Aref";
        var user = new _User { Name = expected, Id = 1 };
        var dto = new _UserDto();
        userMapper.MapTo(user, dto);
        dto.Name.Should().Be(expected);
    }

    [Fact]
    public void CreateDtoWithcustomResolver()
    {
        TypeAdapterConfig.GlobalSettings
            .Scan(Assembly.GetExecutingAssembly());

        var userMapper = GetMappingInterface<IUserMapper>();
        var expected = "Aref";
        var user = new _User { Name = expected, Id = 1 };
        var dto = new _UserDto();
        userMapper.MapTo(user, dto);
        dto.Name.Should().Be(expected);
    }
}


public class User1017
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

public class UserCodeGenConfig : ICodeGenerationRegister
{
    public void Register(CodeGenerationConfig config)
    {
        config.AdaptTo("[name]Dto", MapType.Map)
            .ForType<User1017>(p =>
            {
                p.Ignore(s => s.FirstName);
                p.Map(s => s.LastName, s => $"{s.FirstName} {s.LastName}", "FullName");
            });

        config.GenerateMapper("[name]Mapper")
            .ForType<User1017>();
    }
}

public class UserMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<_User, _UserDto>()
            .MapToConstructor(true)
            .ConstructUsing(s => new _UserDto());
    }
}

public class _User
{
    public int Id { get; init; }
    public string Name { get; init; }
}

public class _UserDto
{
    public int Id { get; init; }
    public string Name { get; init; }
}