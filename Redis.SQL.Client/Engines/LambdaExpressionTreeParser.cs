using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.Engines
{
    internal class LambdaExpressionTreeParser
    {
        private static readonly Type BooleanType = typeof(bool);

        private static readonly Type IntType = typeof(int);

        private static readonly Type LongType = typeof(long);

        private static readonly Type CharType = typeof(char);

        private static readonly Type StringType = typeof(string);

        private static readonly Type DateTimeType = typeof(DateTime);

        private static readonly Type TimeSpanType = typeof(TimeSpan);

        private static readonly IDictionary<Type, Func<MemberInfo, string>> PropertiesEvaluator = 
            new Dictionary<Type, Func<MemberInfo, string>>
            {
                { DateTimeType, EvaluateDateTimeProperties },
                { TimeSpanType, EvaluateTimeSpanProperties },
                { IntType, EvaluateIntProperties },
                { LongType, EvaluateLongProperties },
                { StringType, EvaluateStringProperties }
            };

        private static readonly ExpressionType[] Operators =
        {
            ExpressionType.Equal,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.NotEqual
        };

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

        private static void ConvertNode(BinaryExpression bin, out string lhs, out string rhs)
        {
            var operand = (bin.Left as UnaryExpression)?.Operand;
            var type = operand?.Type;
            if (type == CharType)
            {
                lhs = operand?.ToString();
                rhs = $"'{(char)int.Parse(bin.Right.ToString())}'";
                return;
            }

            throw new LambdaExpressionParsingException();
        }

        private static void EvaluateMember(MemberExpression member, BinaryExpression bin, out string rhs)
        {
            if (member.Expression == null)
            {
                rhs = EvaluateProperty(member);
                return;
            }

            var val = (member.Expression as ConstantExpression)?.Value;

            rhs = val?.GetType().GetFields()[0].GetValue(val)?.ToString();
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

            if (PropertiesEvaluator.TryGetValue(declaringType, out var method))
            {
                return method(memberInfo);
            }

            throw new LambdaExpressionParsingException();
        }

        private static string EvaluateDateTimeProperties(MemberInfo info)
        {
            switch (info.Name)
            {
                case "UtcNow":
                    return $"'{DateTime.UtcNow}'";
                case "Now":
                    return $"'{DateTime.Now}'";
                case "Today":
                    return $"'{DateTime.Today}'";
                case "MaxValue":
                    return $"'{DateTime.MaxValue}'";
                case "MinValue":
                    return $"'{DateTime.MinValue}'";
            }

            throw new LambdaExpressionParsingException();
        }

        private static string EvaluateTimeSpanProperties(MemberInfo info)
        {
            switch (info.Name)
            {
                case "MaxValue":
                    return $"'{TimeSpan.MaxValue}'";
                case "MinValue":
                    return $"'{TimeSpan.MinValue}'";
                case "Zero":
                    return $"'{TimeSpan.Zero}'";
            }

            throw new LambdaExpressionParsingException();
        }

        private static string EvaluateStringProperties(MemberInfo info)
        {
            switch (info.Name)
            {
                case "Empty": return "''";
            }

            throw new LambdaExpressionParsingException();
        }

        private static string EvaluateIntProperties(MemberInfo info)
        {
            switch (info.Name)
            {
                case "MaxValue":
                    return $"'{int.MaxValue}'";
                case "MinValue":
                    return $"'{int.MinValue}'";
            }

            throw new LambdaExpressionParsingException();
        }

        private static string EvaluateLongProperties(MemberInfo info)
        {
            switch (info.Name)
            {
                case "MaxValue":
                    return $"'{long.MaxValue}'";
                case "MinValue":
                    return $"'{long.MinValue}'";
            }

            throw new LambdaExpressionParsingException();
        }
    }
}
