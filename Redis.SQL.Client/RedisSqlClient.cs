﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer.Lexers;
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

        private readonly IDeletionEngine _deletionEngine;

        private readonly IUpdateEngine _updateEngine;

        private readonly ILambdaExpressionTreeParser _lambdaExpressionTreeParser;

        public RedisSqlClient()
        {
            _queryEngine = new RedisSqlQueryEngine();
            _creationEngine = new RedisSqlCreationEngine();
            _lambdaExpressionTreeParser = new LambdaExpressionTreeParser();
            _projectionEngine = new RedisSqlProjectionEngine();
            _insertionEngine = new RedisSqlInsertionEngine();
            _deletionEngine = new RedisSqlDeletionEngine();
            _updateEngine = new RedisSqlUpdateEngine();
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

        public async Task Delete<TEntity>(TEntity entity)
        {
            await _deletionEngine.DeleteEntity(entity);
        }

        public async Task Update<TEntity>(TEntity entity)
        {
            await _updateEngine.UpdateEntity(entity);
        }

        public async Task<IEnumerable<IDictionary<string, string>>> ExecuteSql(string sql)
        {
            sql = sql.Trim();
            var selectKeyword = Keywords.Select.ToString();
            var createKeyword = Keywords.Create.ToString();
            var insertKeyword = Keywords.Insert.ToString();
            var deleteKeyword = Keywords.Delete.ToString();
            var updateKeyword = Keywords.Update.ToString();

            if (sql.StartsWithKeyword(selectKeyword, ' ', '*'))
            {
                return await _projectionEngine.ExecuteSelectStatement(sql);
            }
            
            if (sql.StartsWithKeyword(createKeyword, ' '))
            {
                await _creationEngine.ExecuteCreateStatement(sql);
                goto ReturnEmpty;
            }

            if (sql.StartsWithKeyword(insertKeyword, ' '))
            {
                await _insertionEngine.ExecuteInsertStatement(sql);
                goto ReturnEmpty;
            }

            if (sql.StartsWithKeyword(deleteKeyword, ' '))
            {
                await _deletionEngine.ExecuteDeleteStatement(sql);
                goto ReturnEmpty;
            }

            if (sql.StartsWithKeyword(updateKeyword, ' '))
            {
                await _updateEngine.ExecuteUpdateStatement(sql);
                goto ReturnEmpty;
            }

            throw new SyntacticErrorException(sql);

        ReturnEmpty:
            return new List<IDictionary<string, string>>();
        }
    }
}