using System;
using System.Linq.Expressions;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.Engines
{
    internal class ExpressionTreeParser
    {
        private static readonly Type BooleanType = typeof(bool);

        private static readonly Type CharType = typeof(char);

        private static string ParseExpressionTree(string result, BinaryExpression bin)
        {
            if (bin.NodeType == ExpressionType.OrElse || bin.NodeType == ExpressionType.Or)
            {
                return $"{result}({ParseExpressionTree(result, ToBinary(bin.Left))}) or ({ParseExpressionTree(result, ToBinary(bin.Right))})";
            }

            if (bin.NodeType == ExpressionType.AndAlso || bin.NodeType == ExpressionType.And)
            {
                return $"{result}({ParseExpressionTree(result, ToBinary(bin.Left))}) and ({ParseExpressionTree(result, ToBinary(bin.Right))})";
            }

            if (bin.NodeType == ExpressionType.Equal)
            {
                string rhs = bin.Right.ToString(), lhs = bin.Left.ToString();

                if (bin.Left.NodeType == ExpressionType.Convert)
                {
                    ConvertNode(bin, out rhs, out lhs);
                }

                return $"{result}{lhs} = {rhs.Replace(@"""", "'")}";
            }

            return string.Empty;
        }

        internal string ParseLambdaExpression<TEntity>(Expression<Func<TEntity, bool>> expr)
        {
            return ParseExpressionTree(string.Empty, ToBinary(expr.Body));
        }

        private static BinaryExpression ToBinary(Expression expr)
        {
            if (expr is BinaryExpression bin) return bin;

            if (expr.NodeType == ExpressionType.Not)
            {
                return BooleanExpression((expr as UnaryExpression)?.Operand.ToString(), false);
            }

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                return BooleanExpression((expr as MemberExpression)?.ToString(), true);
            }

            throw new LambdaExpressionParsingException();
        }

        private static BinaryExpression BooleanExpression(string paramName, bool boolean)
        {
            var boolParam = Expression.Parameter(BooleanType, paramName);
            var value = Expression.Constant(boolean, typeof(bool));
            return Expression.Equal(boolParam, value);
        }

        private static void ConvertNode(BinaryExpression bin, out string rhs, out string lhs)
        {
            var operand = (bin.Left as UnaryExpression)?.Operand;
            var type = operand?.Type;
            if (type == CharType)
            {
                rhs = $"'{(char)int.Parse(bin.Right.ToString())}'";
                lhs = operand?.ToString();
                return;
            }

            throw new LambdaExpressionParsingException();
        }
    }
}
