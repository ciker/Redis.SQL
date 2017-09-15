using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.Engines
{
    internal class LambdaExpressionTreeParser
    {
        private static readonly Type BooleanType = typeof(bool);

        private static readonly Type CharType = typeof(char);

        private static readonly Type StringType = typeof(string);

        private static readonly Type DateTimeType = typeof(DateTime);

        private static readonly Type TimeSpanType = typeof(TimeSpan);

        private static readonly ExpressionType[] Operators =
        {
            ExpressionType.Equal,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.NotEqual
        };

        private FieldInfo[] _variables;

        private readonly object _locker = new object();

        internal string ParseLambdaExpression<TEntity>(Expression<Func<TEntity, bool>> expr)
        {
            lock (_locker)
            {
                _variables = null;
                return ParseExpressionTree(string.Empty, ToBinary(expr.Body));
            }
        }

        private string ParseExpressionTree(string result, BinaryExpression bin)
        {
            if (bin.NodeType == ExpressionType.OrElse || bin.NodeType == ExpressionType.Or)
            {
                return $"{result}({ParseExpressionTree(result, ToBinary(bin.Left))}) or ({ParseExpressionTree(result, ToBinary(bin.Right))})";
            }

            if (bin.NodeType == ExpressionType.AndAlso || bin.NodeType == ExpressionType.And)
            {
                return $"{result}({ParseExpressionTree(result, ToBinary(bin.Left))}) and ({ParseExpressionTree(result, ToBinary(bin.Right))})";
            }

            if (Operators.Contains(bin.NodeType))
            {
                string lhs = bin.Left.ToString(), rhs = bin.Right.ToString();

                if (bin.Right is MemberExpression member)
                {
                    EvaluateMember(member, bin, out rhs);
                }
                
                if (bin.Left.NodeType == ExpressionType.Convert)
                {
                    ConvertNode(bin, out lhs, out rhs);
                }

                return $"{result}{lhs} {GetOperator(bin.NodeType)} {rhs?.Replace(@"""", "'")}";
            }

            return string.Empty;
        }

        private static string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal: return "=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.NotEqual: return "!=";
            }

            throw new LambdaExpressionParsingException("Unsupported operator");
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

        private void ConvertNode(BinaryExpression bin, out string lhs, out string rhs)
        {
            var operand = (bin.Left as UnaryExpression)?.Operand;

            var type = operand?.Type;
            if (type == CharType)
            {
                lhs = operand?.ToString();
                if (bin.Right is UnaryExpression unary && unary.Operand is MemberExpression member)
                {
                    EvaluateMember(member, bin, out rhs);
                    rhs = $"'{rhs}'";
                }
                else
                {
                    rhs = $"'{(char)int.Parse(bin.Right.ToString())}'";
                }
                return;
            }

            throw new LambdaExpressionParsingException();
        }
        
        private void EvaluateMember(MemberExpression member, BinaryExpression bin, out string rhs)
        {
            if (member.Expression == null)
            {
                rhs = EvaluateProperty(member);
                return;
            }

            var val = (member.Expression as ConstantExpression)?.Value;
            var variableName = member.Member?.Name;

            if (_variables == null)
            {
                _variables = val?.GetType().GetFields();
            }

            rhs = _variables?.FirstOrDefault(x => x.Name == variableName)?.GetValue(val)?.ToString();
            var leftBinType = bin.Left.Type;

            if (leftBinType == StringType || leftBinType == CharType || leftBinType == DateTimeType || leftBinType == TimeSpanType)
            {
                rhs = $"'{rhs}'";
            }
        }

        private static string EvaluateProperty(MemberExpression member)
        {
            var memberInfo = member.Member;
            var declaringType = memberInfo.DeclaringType;

            var property = declaringType.GetProperty(memberInfo.Name);

            if (property == null)
            {
                var field = declaringType.GetField(memberInfo.Name);
                return $"'{field?.GetValue(null)}'";
            }

            return $"'{property.GetValue(null)}'";
        }
    }
}