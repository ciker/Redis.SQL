using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Engines;

namespace Redis.SQL.Client
{
    public class RedisSqlClient
    {
        private readonly LexicalTokenizer _whereTokenizer;

        private readonly ShiftReduceParser _whereParser;

        private readonly RedisSqlQueryEngine _queryEngine;

        public RedisSqlClient()
        {
            _whereTokenizer = new LexicalTokenizer(Constants.WhereClauseTokenPattern);
            _whereParser = new ShiftReduceParser(Constants.WhereGrammar);
            _queryEngine = new RedisSqlQueryEngine();
        }

        private async Task<IEnumerable<string>> ExecuteWhereStatement(string entityName, string condition)
        {
            var tokens = _whereTokenizer.Tokenize(condition);
            var parseTree = _whereParser.ParseCondition(tokens);
            var keys = await _queryEngine.ExecuteTree(entityName, parseTree);
            ICollection<string> result = new List<string>();
            foreach (var key in keys)
            {
                result.Add(await _queryEngine.RetrieveEntityJsonByKey(entityName, key));
            }
            return result;
        }

        public async Task<IEnumerable<TEntity>> Query<TEntity>(string condition)
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var entities = await ExecuteWhereStatement(entityName, condition);
            return entities.Select(JsonConvert.DeserializeObject<TEntity>).ToList();
        }





        
        public async Task<IEnumerable<TEntity>> Query<TEntity>(Expression<Func<TEntity, bool>> expr)
        {
            var expression = ExpressionTreeParser.ParseExpressionTree(string.Empty, (BinaryExpression)expr.Body);
            var entityName = Helpers.GetTypeName<TEntity>();
            var entities = await ExecuteWhereStatement(entityName, expression);
            return entities.Select(JsonConvert.DeserializeObject<TEntity>).ToList();
        }
    }
}