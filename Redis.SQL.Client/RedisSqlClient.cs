using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Redis.SQL.Client.Engines;
using Redis.SQL.Client.Engines.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.ExpressionTrees;
using Redis.SQL.Client.ExpressionTrees.Interfaces;

namespace Redis.SQL.Client
{
    public class RedisSqlClient
    {
        private readonly IQueryEngine _queryEngine;

        private readonly ICreationEngine _creationEngine;

        private readonly IInsertionEngine _insertionEngine;

        private readonly IProjectionEngine _projectionEngine;

        private readonly ILambdaExpressionTreeParser _lambdaExpressionTreeParser;

        public RedisSqlClient()
        {
            _queryEngine = new RedisSqlQueryEngine();
            _creationEngine = new RedisSqlCreationEngine();
            _lambdaExpressionTreeParser = new LambdaExpressionTreeParser();
            _projectionEngine = new RedisSqlProjectionEngine();
            _insertionEngine = new RedisSqlInsertionEngine();
        }

        public async Task<IEnumerable<TEntity>> Query<TEntity>(string condition)
        {
            return await _queryEngine.QueryEntities<TEntity>(condition);
        }
        
        public async Task<IEnumerable<TEntity>> Query<TEntity>(Expression<Func<TEntity, bool>> expr)
        {
            return await _queryEngine.QueryEntities<TEntity>(_lambdaExpressionTreeParser.ParseLambdaExpression(expr));
        }

        public async Task<IEnumerable<TEntity>> Query<TEntity>()
        {
            return await _queryEngine.QueryEntities<TEntity>(string.Empty);
        }

        public async Task Insert<TEntity>(TEntity entity)
        {
            await _insertionEngine.InsertEntity(entity);
        }

        public async Task Create<TEntity>()
        {
            await _creationEngine.CreateEntity<TEntity>();
        }

        public async Task<IEnumerable<IDictionary<string, string>>> ExecuteSql(string sql)
        {
            sql = sql.Trim();
            var selectKeyword = Keywords.Select.ToString();
            var createKeyword = Keywords.Create.ToString();

            if (sql.StartsWith($"{selectKeyword.ToLower()} ", StringComparison.OrdinalIgnoreCase))
            {
                return await _projectionEngine.ExecuteSelectStatement(sql);
            }

            if (sql.StartsWith($"{createKeyword.ToLower()} ", StringComparison.OrdinalIgnoreCase))
            {
                await _creationEngine.ExecuteCreateStatement(sql);
                return new List<IDictionary<string, string>>();
            }

            throw new SyntacticErrorException(sql);
        }
    }
}