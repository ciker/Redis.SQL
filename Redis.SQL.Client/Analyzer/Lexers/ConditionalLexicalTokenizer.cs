using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class ConditionalLexicalTokenizer : ILexer
    {
        public IList<string> Tokenize(string condition)
        {
            IList<string> result = new List<string>();
            var stringParam = false;
            var token = string.Empty;

            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    token += '\'';
                    stringParam = !stringParam;
                    if (!stringParam)
                        token = result.AddLexicalToken(token, Constants.WhereClauseTokenPattern);
                    continue;
                }

                if (stringParam)
                {
                    token += condition[i];
                    continue;
                }

                if (condition[i] == ' ') continue;

                if (condition[i] == '(' || condition[i] == ')')
                {
                    result.AddLexicalToken(token, Constants.WhereClauseTokenPattern);
                    token = result.AddLexicalToken(condition[i].ToString(), Constants.WhereClauseTokenPattern);
                    continue;
                }

                if (IsKeyword(condition.Substring(i), out var keyword))
                {
                    result.AddLexicalToken(token, Constants.WhereClauseTokenPattern);
                    token = result.AddLexicalToken($" {keyword} ", Constants.WhereClauseTokenPattern);
                    i += keyword.Length - 1;
                    continue;
                }

                token += condition[i];
            }

            result.AddLexicalToken(token, Constants.WhereClauseTokenPattern);
            return result;
        }

        private static bool IsKeyword(string condition, out string keyword)
        {
            keyword = null;
            condition = condition.ToLower();
            string andKeyword = Keywords.And.ToString().ToLower(), orKeyword = Keywords.Or.ToString().ToLower();

            if (condition.StartsWith(andKeyword + " ") || condition.StartsWith(andKeyword + "("))
            {
                keyword = andKeyword;
                return true;
            }

            if (condition.StartsWith(orKeyword + " ") || condition.StartsWith(orKeyword + "("))
            {
                keyword = orKeyword;
                return true;
            }

            return false;
        }
    }
}