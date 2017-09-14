using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class ConditionalLexicalTokenizer : ILexer
    {
        public IEnumerable<string> Tokenize(string condition)
        {
            ICollection<string> result = new List<string>();
            var stringParam = false;
            var token = string.Empty;

            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    token += '\'';
                    stringParam = !stringParam;
                    if (!stringParam)
                        token = AddToken(token, result);
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
                    AddToken(token, result);
                    token = AddToken(condition[i].ToString(), result);
                    continue;
                }

                if (IsKeyword(condition.Substring(i), out var keyword))
                {
                    AddToken(token, result);
                    token = AddToken(" " + keyword + " ", result);
                    i += keyword.Length - 1;
                    continue;
                }

                token += condition[i];
            }

            AddToken(token, result);
            return result;
        }

        private static string AddToken(string token, ICollection<string> tokens)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            if (!Regex.IsMatch(token.Trim(), Constants.WhereClauseTokenPattern))
            {
                throw new SyntacticErrorException(token);
            }
            tokens.Add(token);
            return string.Empty;
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