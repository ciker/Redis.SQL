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

        private readonly BinaryTree<string> _parsingTree = new BinaryTree<string>();

        private void Tokenize(string condition)
        {
            var stringParam = false;
            var token = string.Empty;

            var currentNode = _parsingTree;

            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    token += '\'';
                    stringParam = !stringParam;
                    if (!stringParam)
                    {
                        token = AddToken(currentNode, token);
                    }
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
                    currentNode.AddChild(child);
                    currentNode = child;
                    continue;
                }

                if (condition[i] == ')')
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        token = AddToken(currentNode, token);
                    }
                    currentNode = currentNode.Parent;
                    continue;
                }

                if (IsAndOperator(condition.Substring(i)))
                {
                    currentNode.SetValue(Keywords.And.ToString());
                    token = AddToken(currentNode, token);
                    i += 2;
                    continue;
                }

                if (IsOrOperator(condition.Substring(i)))
                {
                    currentNode.SetValue(Keywords.Or.ToString());
                    token = AddToken(currentNode, token);
                    i++;
                    continue;
                }
                
                token += condition[i];
            }
        }

        private static string AddToken(BinaryTree<string> currentNode, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            currentNode.AddChild(new BinaryTree<string>(token));
            return string.Empty;
        }

        private static bool IsOrOperator(string condition)
        {
            condition = condition.ToLower();
            var orString = Keywords.Or.ToString().ToLower();
            return condition.StartsWith(orString + " ") || condition.StartsWith(orString + "(");
        }

        private static bool IsAndOperator(string condition)
        {
            condition = condition.ToLower();
            var andString = Keywords.And.ToString().ToLower();
            return condition.StartsWith(andString + " ") || condition.StartsWith(andString + "(");
        }







        public async Task ParseCondition(string entityName, string condition)
        {
            Tokenize(condition);

            foreach (var item in _parsingTree)
            {
                if (item.Value == null)
                {
                    
                }
            }

            var s = _parsingTree.Where(x => x.Value != null);
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