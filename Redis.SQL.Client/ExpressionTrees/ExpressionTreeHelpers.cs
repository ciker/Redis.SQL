using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.ExpressionTrees
{
    internal class ExpressionTreeHelpers
    {
        internal static readonly Type BooleanType = typeof(bool);

        internal static readonly Type CharType = typeof(char);

        internal static readonly Type StringType = typeof(string);

        internal static readonly Type DateTimeType = typeof(DateTime);

        internal static readonly Type TimeSpanType = typeof(TimeSpan);

        internal static readonly ExpressionType[] Operators =
        {
            ExpressionType.Equal,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.NotEqual
        };

        private static readonly IDictionary<string, Type> CachedTypes = new Dictionary<string, Type>();

        internal static BinaryExpression BooleanToBinaryExpression(string paramName, bool boolean)
        {
            var boolParam = Expression.Parameter(BooleanType, paramName);
            var value = Expression.Constant(boolean, typeof(bool));
            return Expression.Equal(boolParam, value);
        }

        internal static BinaryExpression ToBinary(Expression expr)
        {
            if (expr is BinaryExpression bin) return bin;

            if (expr.NodeType == ExpressionType.Not)
            {
                return BooleanToBinaryExpression((expr as UnaryExpression)?.Operand.ToString(), false);
            }

            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                return BooleanToBinaryExpression((expr as MemberExpression)?.ToString(), true);
            }

            throw new LambdaExpressionParsingException();
        }

        internal static string GetOperator(ExpressionType type)
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


        internal static ConstantExpression GetConstantExpression(MemberExpression member)
        {
            var expression = member.Expression;

            while (true)
            {
                if (expression is ConstantExpression constant)
                {
                    return constant;
                }

                expression = (expression as MemberExpression)?.Expression;

                if (expression == null) return null;
            }
        }

        internal static IEnumerable<string> GetAccessVariables(MemberExpression member)
        {
            var result = new Stack<string>();

            while (true)
            {
                result.Push(member.Member.Name);

                switch (member.Expression)
                {
                    case null: return result;
                    case MemberExpression expression: member = expression; break;
                    default: return result;
                }
            }
        }

        internal static Type CachedReflection(object obj)
        {
            var identifier = obj.ToString();
            if (CachedTypes.TryGetValue(identifier, out var type))
            {
                return type;
            }

            var reflection = obj.GetType();
            CachedTypes.Add(identifier, reflection);
            return reflection;
        }

        internal static string EvaluateProperty(MemberExpression member)
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
