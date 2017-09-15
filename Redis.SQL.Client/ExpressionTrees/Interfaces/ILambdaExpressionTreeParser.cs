using System;
using System.Linq.Expressions;

namespace Redis.SQL.Client.ExpressionTrees.Interfaces
{
    internal interface ILambdaExpressionTreeParser
    {
        string ParseLambdaExpression<TEntity>(Expression<Func<TEntity, bool>> expr);
    }
}
