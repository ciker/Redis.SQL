using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.ExpressionTrees.Interfaces;

namespace Redis.SQL.Client.ExpressionTrees
{
    internal class LambdaExpressionTreeParser : ILambdaExpressionTreeParser
    {
        private FieldInfo[] _variables;

        private readonly object _locker;

        internal LambdaExpressionTreeParser()
        {
            _locker = new object();
            _variables = null;
        }

        public string ParseLambdaExpression<TEntity>(Expression<Func<TEntity, bool>> expr)
        {
            lock (_locker)
            {
                _variables = null;
                return ParseExpressionTree(string.Empty, ExpressionTreeHelpers.ToBinary(expr.Body));
            }
        }

        private string ParseExpressionTree(string result, BinaryExpression bin)
        {
            if (bin.NodeType == ExpressionType.OrElse || bin.NodeType == ExpressionType.Or)
            {
                return $"{result}({ParseExpressionTree(result, ExpressionTreeHelpers.ToBinary(bin.Left))}) or ({ParseExpressionTree(result, ExpressionTreeHelpers.ToBinary(bin.Right))})";
            }

            if (bin.NodeType == ExpressionType.AndAlso || bin.NodeType == ExpressionType.And)
            {
                return $"{result}({ParseExpressionTree(result, ExpressionTreeHelpers.ToBinary(bin.Left))}) and ({ParseExpressionTree(result, ExpressionTreeHelpers.ToBinary(bin.Right))})";
            }

            if (ExpressionTreeHelpers.Operators.Contains(bin.NodeType))
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

                if (bin.Right.NodeType == ExpressionType.Not)
                {
                    rhs = HandleNotExpression(bin);
                }

                return $"{result}{lhs} {ExpressionTreeHelpers.GetOperator(bin.NodeType)} {rhs?.Replace(@"""", "'")}";
            }

            return string.Empty;
        }

        internal string HandleNotExpression(BinaryExpression bin)
        {
            var negated = true;

            if (bin.Right is UnaryExpression unary)
            {
                var exp = unary.Operand;
                while (exp is UnaryExpression sub) //Handling multiple negations
                {
                    if (sub.NodeType == ExpressionType.Not)
                    {
                        negated = !negated;
                    }
                    exp = sub.Operand;
                }

                if (exp is MemberExpression member)
                {
                    EvaluateMember(member, bin, out var result);
                    return $"{bool.Parse(result) && !negated}";
                }
            }

            return null;
        }

        private void ConvertNode(BinaryExpression bin, out string lhs, out string rhs)
        {
            var operand = (bin.Left as UnaryExpression)?.Operand;

            var type = operand?.Type;
            if (type == ExpressionTreeHelpers.CharType)
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

        private string GetMemberValue(MemberExpression member, ConstantExpression constant)
        {
            var accessVariables = ExpressionTreeHelpers.GetAccessVariables(member).ToList();

            if (!accessVariables.Any())
            {
                throw new LambdaExpressionParsingException();
            }

            var obj = _variables?.FirstOrDefault(x => x.Name == accessVariables[0])?.GetValue(constant.Value);

            accessVariables.RemoveAt(0);

            foreach (var variable in accessVariables)
            {
                var type = ExpressionTreeHelpers.CachedReflection(obj);
                obj = type.GetField(variable)?.GetValue(obj) ?? type.GetProperty(variable)?.GetValue(obj);
            }

            return obj?.ToString();
        }

        private void EvaluateMember(MemberExpression member, BinaryExpression bin, out string rhs)
        {
            if (member.Expression == null)
            {
                rhs = ExpressionTreeHelpers.EvaluateProperty(member);
                return;
            }

            var constant = ExpressionTreeHelpers.GetConstantExpression(member);

            if (constant == null)
            {
                throw new LambdaExpressionParsingException();
            }

            var val = constant.Value;

            if (_variables == null)
            {
                _variables = val?.GetType().GetFields();
            }

            rhs = GetMemberValue(member, constant);

            var leftBinType = bin.Left.Type;

            if (leftBinType == ExpressionTreeHelpers.StringType || leftBinType == ExpressionTreeHelpers.CharType 
                || leftBinType == ExpressionTreeHelpers.DateTimeType || leftBinType == ExpressionTreeHelpers.TimeSpanType)
            {
                rhs = $"'{rhs}'";
            }
        }
    }
}