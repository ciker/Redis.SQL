using System;
using System.Collections.Generic;
using System.Linq;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Parsers
{
    internal class UpdateParser : ICustomizedParser
    {
        public BaseModel ParseTokens(IList<string> tokens)
        {
            tokens.RemoveAt(0);

            var result = new UpdateModel
            {
                UpdatedProperties = new Dictionary<string, string>()
            };

            bool entityNameParsed = false, setKeywordParsed = false, whereKeywordParsed = false;
            string setKeyword = Keywords.Set.ToString(), whereKeyword = Keywords.Where.ToString();

            while (tokens.Count > 0)
            {
                var token = tokens[0];
                if (!entityNameParsed)
                {
                    result.EntityName = token;
                    entityNameParsed = true;
                }

                if (setKeywordParsed && !whereKeywordParsed && token != whereKeyword)
                {
                    var split = token.Split('=');
                    var property = split[0];
                    var value = string.Join(string.Empty, split.Skip(1));
                    result.UpdatedProperties.Add(property.ToLower(), value);
                }

                if (token == setKeyword && !setKeywordParsed)
                {
                    setKeywordParsed = true;
                }

                if (whereKeywordParsed)
                {
                    result.WhereCondition = token;
                }

                if (setKeywordParsed && token == whereKeyword)
                {
                    whereKeywordParsed = true;
                }

                tokens.RemoveAt(0);
            }

            return result;
        }
    }
}
