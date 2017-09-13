using System.Linq.Expressions;

namespace Redis.SQL.Client.Engines
{
    internal class ExpressionTreeParser
    {
        internal static string ParseExpressionTree(string result, BinaryExpression bin)
        {
            if (bin.NodeType == ExpressionType.OrElse || bin.NodeType == ExpressionType.Or)
            {
                return $"{result} ({ParseExpressionTree(result, bin.Left as BinaryExpression)}) or ({ParseExpressionTree(result, bin.Right as BinaryExpression)})";
            }

            if (bin.NodeType == ExpressionType.AndAlso || bin.NodeType == ExpressionType.And)
            {
                return $"{result} ({ParseExpressionTree(result, bin.Left as BinaryExpression)}) and ({ParseExpressionTree(result, bin.Right as BinaryExpression)})";
            }

            if (bin.NodeType == ExpressionType.Equal)
            {
                return $"{result} {bin.Left} = {bin.Right.ToString().Replace(@"""", "'")}";
            }

            return string.Empty;
        }

        internal static string ResolveBoolean(Expression expr) => expr.NodeType == ExpressionType.Not ? $"{expr} = false" : expr.ToString().StartsWith("!") ? $"{expr} = false" : $"{expr} = true";
    }
}
