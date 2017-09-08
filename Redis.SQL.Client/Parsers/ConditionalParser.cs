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

        private readonly ConditionalTokenizer _conditionalTokenizer;

        private readonly string[] _operations = { ">=", "<=", ">", "<", "!=", "=" };

        public ConditionalParser()
        {
            _queryEngine = new RedisSqlQueryEngine();
            _conditionalTokenizer = new ConditionalTokenizer();
        }

        public async Task ParseCondition(string entityName, string condition)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var parseTree = _conditionalTokenizer.Tokenize(condition);
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