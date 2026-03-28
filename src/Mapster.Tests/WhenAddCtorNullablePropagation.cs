using Mapster.Tests.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddCtorNullablePropagation
    {

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/898
        /// </summary>
        [TestMethod]
        public void NullablePropagationFromCtorWorking()
        {
            var source = new List<OrderEntity898>();

            source.Add(new OrderEntity898() { Id = 1, Cod = new OrderCodEntity898 { Value = 42L } });
            source.Add(new OrderEntity898() { Id = 2, Cod = null });

            var str = new OrderEntity898() { Id = 1, Cod = new OrderCodEntity898 { Value = 42L } }.BuildAdapter().CreateProjectionExpression<OrderDto898>();

            var result = source.AsQueryable().ProjectToType<OrderDto898>().ToList();
        }

    }

    #region TestClasses

    public record OrderDto898(int Id, OrderCodDto898? Cod);
    public record OrderCodDto898(long Value);


    public class OrderEntity898
    {
        public int Id { get; set; }
        public int? CodId { get; set; }
        public OrderCodEntity898? Cod { get; set; }
    }

    public class OrderCodEntity898
    {
        public int Id { get; set; }
        public long Value { get; set; }
    }


    #endregion TestClasses
}
