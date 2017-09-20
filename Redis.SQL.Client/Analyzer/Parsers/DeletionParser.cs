using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Parsers
{
    internal class DeletionParser : ICustomizedParser
    {
        public BaseModel ParseTokens(IList<string> tokens)
        {
            var model = new DeletionModel();
            tokens.RemoveAt(0);

            var whereKeyword = Keywords.Where.ToString();

            var whereKeywordParsed = false;

            while (tokens.Count > 0)
            {
                var token = tokens[0];

                if (!whereKeywordParsed)
                {
                    model.EntityName = token;
                    whereKeywordParsed = true;
                }
                else
                {
                    model.WhereCondition = token;
                }
                
                tokens.RemoveAt(0);
            }

            return model;
        }
    }
}
