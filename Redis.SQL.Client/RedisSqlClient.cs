using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Engines;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client
{
    public class RedisSqlClient
    {
        private readonly ILexer _conditionalTokenizer;

        private readonly ILexer _projectionalTokenizer;

        private readonly ShiftReduceParser _whereParser;

        private readonly ProjectionalParser _projectionalParser;

        private readonly RedisSqlQueryEngine _queryEngine;

        public RedisSqlClient()
        {
            _conditionalTokenizer = new ConditionalLexicalTokenizer();
            _projectionalTokenizer = new ProjectionalLexicalTokenizer();
            _whereParser = new ShiftReduceParser(Constants.WhereGrammar);
            _projectionalParser = new ProjectionalParser();
            _queryEngine = new RedisSqlQueryEngine();
        }

        private async Task<IEnumerable<IDictionary<string, string>>> ExecuteSelectStatement(string sql)
        {
            var tokens = _projectionalTokenizer.Tokenize(sql).ToList();
            var projectionalModel = _projectionalParser.ParseSelectStatement(tokens);
            var entities = await ExecuteWhereStatement(projectionalModel.EntityName, projectionalModel.Query);
            var deserialized = entities.Select(JsonConvert.DeserializeObject<dynamic>).ToList();
            var result = new List<IDictionary<string, string>>();
            foreach (var item in deserialized)
            {
                var dictionary = new Dictionary<string, string>();

                if (projectionalModel.ProjectAllProperties)
                {
                 //PROJECT ALL   
                }
                foreach (var prop in projectionalModel.ProjectedProperties)
                {
                    dictionary.Add(prop, item[prop].ToString());
                }
                result.Add(dictionary);
            }
            return result;
        }

        private async Task<IEnumerable<string>> ExecuteWhereStatement(string entityName, string condition)
        {
            IEnumerable<string> keys;

            if (string.IsNullOrEmpty(condition))
            {
                keys = await _queryEngine.GetAllEntitykeys(entityName);
            }
            else
            {
                var tokens = _conditionalTokenizer.Tokenize(condition);
                var parseTree = _whereParser.ParseCondition(tokens);
                keys = await _queryEngine.ExecuteTree(entityName, parseTree);
            }

            ICollection<string> result = new List<string>();

            foreach (var key in keys)
            {
                result.Add(await _queryEngine.RetrieveEntityJsonByKey(entityName, key));
            }

            return result;
        }

        private async Task<IEnumerable<TEntity>> QueryEntity<TEntity>(string condition)
        {
            var entityName = Helpers.GetTypeName<TEntity>();
            var entities = await ExecuteWhereStatement(entityName, condition);
            return entities.Select(JsonConvert.DeserializeObject<TEntity>).ToList();
        }

        public async Task<IEnumerable<TEntity>> Query<TEntity>(string condition)
        {
            return await QueryEntity<TEntity>(condition);
        }
        
        public async Task<IEnumerable<TEntity>> Query<TEntity>(Expression<Func<TEntity, bool>> expr)
        {
            var expression = ExpressionTreeParser.ParseExpressionTree(string.Empty, (BinaryExpression)expr.Body);
            return await QueryEntity<TEntity>(expression);
        }

        public async Task<IEnumerable<dynamic>> ExecuteSql(string sql)
        {
            sql = sql.Trim();
            var selectKeyword = Keywords.Select.ToString();
            if (sql.StartsWith(selectKeyword.ToLower() + " ", StringComparison.OrdinalIgnoreCase))
            {
                ExecuteSelectStatement(sql);
            }
            return null;
        }
    }
}