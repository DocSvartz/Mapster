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
    public async Task NotNullableStructMapToNotNullableCorrect()
    {
         TypeAdapterConfig<Foo858, Bar858>
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
        var bar = foo.Adapt<Bar858>();
        // Assert
        bar.InnerAmount.Amount.ShouldBe(10m);
    }

    [TestMethod]
    public async Task NotNullableStructMapToNullableCorrect()
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

}

#region TestClasses
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