using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Engines;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Parsers
{
    public class ConditionalParser
    {
        private readonly RedisSqlQueryEngine _queryEngine;

        public ConditionalParser()
        {
            _queryEngine = new RedisSqlQueryEngine();
        }

        private readonly string[] _operations = {">=", "<=", ">", "<", "!=", "="};

        private BinaryTree<string> _parseTree = new BinaryTree<string>();

        private void Tokenize(string condition)
        {
            var stringParam = false;
            string token = string.Empty, lastOperator = string.Empty;

            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    token += '\'';
                    stringParam = !stringParam;
                    if (!stringParam)
                        token = AddToken(_parseTree, token);
                    continue;
                }

                if (stringParam)
                {
                    token += condition[i];
                    continue;
                }

                if (condition[i] == ' ') continue;

                if (condition[i] == '(')
                {
                    var child = new BinaryTree<string>();
                    _parseTree.AddChild(child);
                    _parseTree = child;
                    continue;
                }

                if (condition[i] == ')')
                {
                    if (!string.IsNullOrEmpty(token))
                        token = AddToken(_parseTree, token);
                    _parseTree = _parseTree.Parent;
                    continue;
                }

                if (IsKeyword(condition.Substring(i), out var keyword))
                {
                    lastOperator = keyword;
                    token = AddToken(_parseTree, token);
                    if (!string.IsNullOrWhiteSpace(_parseTree.Value))
                    {
                        _parseTree = _parseTree.Parent ?? (_parseTree.Parent = new BinaryTree<string> {LeftChild = _parseTree});
                    }
                    _parseTree.SetValue(keyword);
                    i += keyword.Length - 1;
                    continue;
                }

                token += condition[i];
            }

            if (!string.IsNullOrWhiteSpace(token))
                _parseTree.Value = lastOperator;

            AddToken(_parseTree, token);

            while (!_parseTree.IsRoot())
                _parseTree = _parseTree.Parent;
        }

        private static string AddToken(BinaryTree<string> currentNode, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            currentNode.AddChild(new BinaryTree<string>(token));
            return string.Empty;
        }

        private static bool IsKeyword(string condition, out string keyword)
        {
            keyword = null;
            condition = condition.ToLower();
            string andKeyword = Keywords.And.ToString().ToLower(), orKeyword = Keywords.Or.ToString().ToLower();

            if (condition.StartsWith(andKeyword + " ") || condition.StartsWith(andKeyword + "("))
            {
                keyword = andKeyword;
                return true;
            }

            if (condition.StartsWith(orKeyword + " ") || condition.StartsWith(orKeyword + "("))
            {
                keyword = orKeyword;
                return true;
            }

            return false;
        }







        public async Task ParseCondition(string entityName, string condition)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Tokenize(condition);
            watch.Stop();
            var s = watch.ElapsedMilliseconds;
            IList<string> clauses = new List<string>();
            IList <IEnumerable<string>> targetKeys = new List<IEnumerable<string>>();






            
            while (clauses.Any())
            {
                var clause = clauses.First();

                for (var i = 0; i < _operations.Length; i++)
                {
                    var operation = _operations[i];
                    var property = clause.Split(new []{ operation }, StringSplitOptions.RemoveEmptyEntries).First().Trim();

                    if (string.Equals(property, clause, StringComparison.OrdinalIgnoreCase) || property.Any(x => x == '\'')) continue;

                    var value = clause.Substring(clause.IndexOf(operation, StringComparison.OrdinalIgnoreCase) + operation.Length).Trim('\'');
                    targetKeys.Add(await _queryEngine.ExecuteCondition(entityName, property, (Operator)Math.Pow(2D, i), value));
                    break;
                }
                clauses.RemoveAt(0);
            }
        }
    }
}