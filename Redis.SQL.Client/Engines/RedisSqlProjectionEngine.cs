using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Analyzer.Lexers;
using Redis.SQL.Client.Analyzer.Parsers;
using Redis.SQL.Client.Engines.Interfaces;

namespace Redis.SQL.Client.Engines
{
    internal class RedisSqlProjectionEngine : IProjectionEngine
    {
        private readonly ILexer _projectionalTokenizer;

        private readonly IProjectionalParser _projectionalParser;

        private readonly IQueryEngine _queryEngine;

        internal RedisSqlProjectionEngine()
        {
            _projectionalTokenizer = new ProjectionalLexicalTokenizer();
            _projectionalParser = new ProjectionalParser();
            _queryEngine = new RedisSqlQueryEngine();
        }

        public async Task<IEnumerable<IDictionary<string, string>>> ExecuteSelectStatement(string selectStatement)
        {
            var tokens = _projectionalTokenizer.Tokenize(selectStatement).ToList();
            var projectionalModel = _projectionalParser.ParseSelectStatement(tokens);
            var queryResult = await _queryEngine.QueryEntities(projectionalModel.EntityName, projectionalModel.Query);
            var result = new List<IDictionary<string, string>>();
            foreach (var item in queryResult)
            {
                var dictionary = new Dictionary<string, string>();

                foreach (var prop in item)
                {
                    var name = prop.Name;

                    if (!projectionalModel.ProjectAllProperties && projectionalModel.ProjectedProperties.All(x => !string.Equals(x, name, StringComparison.OrdinalIgnoreCase))) continue;

                    var value = (prop as IEnumerable<dynamic>)?.FirstOrDefault()?.Value;

                    if (value != null && !dictionary.TryGetValue(name, out string _))
                    {
                        dictionary.Add(name, value.ToString());
                    }
                }

                result.Add(dictionary);
            }
            return result;
        }
    }
}
