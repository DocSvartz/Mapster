using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Threading.Tasks;

namespace Mapster.Tests;

[TestClass]
public class WhenPropertyNullablePropagationRegression
{
    /// <summary>
    /// https://github.com/MapsterMapper/Mapster/issues/858
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public void NotNullableStructMapToNotNullableCorrect()
    {
        TypeAdapterConfig<Foo858, Bar858>
          .NewConfig()
          .Map(dest => dest.Amount, src => src.Amount)
          .Map(dest => dest.InnerAmount, src => src.Inner.Amount);

        TypeAdapterConfig<Bar858, Bar858>
             .NewConfig()
             .IgnoreNullValues(true);


        Foo858 foo = new()
        {
            Amount = new(1, Currency858.Usd),
            Inner = new()
            {
                Amount = new(10, Currency858.Eur),
                Int = 100,
            }
        };

        // Act
        var bar = foo.Adapt<Bar858>();

        // Assert
        bar.InnerAmount.Amount.ShouldBe(10m);
    }

    [TestMethod]
    public void NotNullableStructMapToNullableCorrect()
    {
        TypeAdapterConfig<Foo858, Bar858Nullable>
            .NewConfig()
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.InnerAmount, src => src.Inner.Amount);


        Foo858 foo = new()
        {
            Amount = new(1, Currency858.Usd),
            Inner = new()
            {
                Amount = new(10, Currency858.Eur),
                Int = 100,
            }
        };

        // Act
        var bar = foo.Adapt<Bar858Nullable>();
        // Assert
        bar.InnerAmount?.Amount.ShouldBe(10m);
    }

    [TestMethod]
    public void IgnoreNullValueWorkCorrect()
    {
        TypeAdapterConfig<Foo858, Bar858>
          .NewConfig()
          .IgnoreNullValues(true)
          .Map(dest => dest.Amount, src => src.Amount)
          .Map(dest => dest.InnerAmount, src => src.Inner.Amount);

        Foo858 foo = new()
        {
            Amount = new(1, Currency858.Usd),
            Inner = new()
            {
                Amount = new(10, Currency858.Eur),
                Int = 100,
            }
        };

        var nullFoo = new Foo858() { Amount = new(2, Currency858.Ron), Inner = null };

        // Act
        var bar = foo.Adapt<Bar858>();
        nullFoo.Adapt(bar);

        // Assert
        bar.InnerAmount.Amount.ShouldBe(10m);
        bar.Amount.Amount.ShouldBe(2m);
        bar.Amount.Currency.ShouldBe(Currency858.Ron);
    }

    [TestMethod]
    public void MapToTargetWorkCorrect()
    {
        TypeAdapterConfig<Foo858, Bar858>
          .NewConfig()
          .IgnoreNullValues(true)
          .Map(dest => dest.Amount, src => src.Amount)
          .Map(dest => dest.InnerAmount, src => src.Inner.Amount);

        Foo858 foo = new()
        {
            Amount = new(1, Currency858.Usd),
            Inner = new()
            {
                Amount = new(10, Currency858.Eur),
                Int = 100,
            }
        };

        var nullFoo = new Foo858() { Amount = new(2, Currency858.Ron), Inner = new()
        {
            Amount = new(20, Currency858.Eur),
            Int = 100,
        }
        };

        // Act
        var bar = foo.Adapt<Bar858>();
        nullFoo.Adapt(bar);

        // Assert
        bar.InnerAmount.Amount.ShouldBe(20m);
        bar.Amount.Amount.ShouldBe(2m);
        bar.Amount.Currency.ShouldBe(Currency858.Ron);
    }


    /// <summary>
    /// https://github.com/MapsterMapper/Mapster/issues/987
    /// </summary>
    [TestMethod]
    public void CustomMapUsingNullableValueTypesWorkCorrect()
    {
        var config = new TypeAdapterConfig();
        config.ForType<SourceClass987, DestinationClass987>()
            .Map(dest => dest.Value, src => src.UseSecondaryValue
                ? src.SecondaryValue1 + src.SecondaryValue2
                : src.PrimaryValue);


        var source1 = new SourceClass987
        {
            UseSecondaryValue = false,
            PrimaryValue = 100
        };
        var source2 = new SourceClass987
        {
            UseSecondaryValue = true,
            SecondaryValue1 = 10,
            SecondaryValue2 = 20
        };

        var result1 = source1.Adapt<DestinationClass987>(config);
        var result2 = source2.Adapt<DestinationClass987>(config);

        result1.Value.ShouldBe(100);
        result2.Value.ShouldBe(30);
    }

}

#region TestClasses

public class SourceClass987
{
    public bool UseSecondaryValue { get; set; }
    public int? PrimaryValue { get; set; }
    public int? SecondaryValue1 { get; set; }
    public int? SecondaryValue2 { get; set; }
}

public class DestinationClass987
{
    public int Value { get; set; }
}

public enum Currency858
{
    Eur,
    Usd,
    Ron
}

file class Foo858
{
    public required Money858 Amount { get; set; }
    public required FooInner858 Inner { get; set; }
}

file class FooInner858
{
    public required Money858 Amount { get; set; }
    public int Int { get; set; }
}

file class Bar858
{
    public  Money858 Amount { get; set; }
    public  Money858 InnerAmount { get; set; }
  
}

file class Bar858Nullable
{
    public Money858? Amount { get; set; }
    public Money858? InnerAmount { get; set; }

}

public struct Money858
{
    public decimal? Amount { get; set; }

    public Currency858 Currency { get; set; } = Currency858.Ron;

    public Money858(decimal? amount, Currency858 currency = Currency858.Eur)
    {
        Amount = amount;
        Currency = currency;
    }
}

#endregion TestClasses