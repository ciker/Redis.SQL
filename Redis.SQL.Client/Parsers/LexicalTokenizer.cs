using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Parsers
{
    internal class LexicalTokenizer
    {
        private readonly ICollection<string> _tokens;

        private readonly string _pattern;

        internal LexicalTokenizer(string pattern)
        {
            _pattern = pattern;
            _tokens = new List<string>();
        }

        internal IEnumerable<string> Tokenize(string condition)
        {
            var stringParam = false;
            var token = string.Empty;

            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    token += '\'';
                    stringParam = !stringParam;
                    if (!stringParam)
                        token = AddToken(token);
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
                    AddToken(token);
                    token = AddToken(condition[i].ToString());
                    continue;
                }

                if (IsKeyword(condition.Substring(i), out var keyword))
                {
                    AddToken(token);
                    token = AddToken(" " + keyword + " ");
                    i += keyword.Length - 1;
                    continue;
                }

                token += condition[i];
            }

            AddToken(token);
            return _tokens;
        }

        private string AddToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            if (!Regex.IsMatch(token, _pattern))
            {
                throw new SyntacticErrorException("Error Parsing: " + token);
            }
            _tokens.Add(token);
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