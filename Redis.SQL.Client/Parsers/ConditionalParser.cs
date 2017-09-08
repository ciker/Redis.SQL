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

        private BinaryTree<string> _parsingTree = new BinaryTree<string>();

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
                        token = AddToken(_parsingTree, token);
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
                    _parsingTree.AddChild(child);
                    _parsingTree = child;
                    continue;
                }

                if (condition[i] == ')')
                {
                    if (!string.IsNullOrEmpty(token))
                        token = AddToken(_parsingTree, token);
                    _parsingTree = _parsingTree.Parent;
                    continue;
                }

                if (IsKeyword(Keywords.And, condition.Substring(i)))
                {
                    lastOperator = Keywords.And.ToString();
                    _parsingTree.SetValue(Keywords.And.ToString());
                    token = AddToken(_parsingTree, token);
                    i += Keywords.And.ToString().Length - 1;
                    continue;
                }

                if (IsKeyword(Keywords.Or, condition.Substring(i)))
                {
                    lastOperator = Keywords.Or.ToString();
                    _parsingTree.SetValue(Keywords.Or.ToString());
                    token = AddToken(_parsingTree, token);
                    i += Keywords.Or.ToString().Length - 1;
                    continue;
                }
                
                token += condition[i];
            }

            if (!string.IsNullOrWhiteSpace(token))
                _parsingTree.Value = lastOperator;

            AddToken(_parsingTree, token);

            while (!_parsingTree.IsRoot())
                _parsingTree = _parsingTree.Parent;
        }

        private static string AddToken(BinaryTree<string> currentNode, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            currentNode.AddChild(new BinaryTree<string>(token));
            return string.Empty;
        }

        private static bool IsKeyword(Keywords keyword, string condition)
        {
            condition = condition.ToLower();
            var lowerKeyword = keyword.ToString().ToLower();
            return condition.StartsWith(lowerKeyword + " ") || condition.StartsWith(lowerKeyword + "(");
        }







        public async Task ParseCondition(string entityName, string condition)
        {
            Tokenize(condition);

            IList<string> clauses = new List<string>();
            IList <IEnumerable<string>> targetKeys = new List<IEnumerable<string>>();


            Stopwatch watch = new Stopwatch();




            
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