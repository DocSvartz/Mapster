using System;
using System.Linq.Expressions;

namespace Mapster.Models
{
    public class FuncParamsGroup
    {
        public FuncParamsGroup(Type typeOfParam) 
        {
            Source = Expression.Parameter(typeOfParam, "p1");
            Destination = Expression.Parameter(typeOfParam,"p2");
            Result = Expression.Parameter(typeOfParam,"result");
        }

        public ParameterExpression Source { get; }
        public ParameterExpression Destination { get; }
        public ParameterExpression Result { get; }
    }
}
