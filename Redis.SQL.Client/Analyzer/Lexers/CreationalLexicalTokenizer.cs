using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class CreationalLexicalTokenizer : ILexer
    {
        public IEnumerable<string> Tokenize(string statement)
        {
            var tokens = new List<string>();

            var createKeyword = Keywords.Create.ToString();

            var token = string.Empty;
            for (var i = 0; i < statement.Length; i++)
            {
                if (statement[i] == ' ') continue;
                if (statement.Substring(i).StartsWith($"{createKeyword} ", StringComparison.OrdinalIgnoreCase))
                {
                    i += createKeyword.Length - 1;
                    tokens.Add(createKeyword.ToLower());
                    continue;
                }

                if (statement[i] == '(' || statement[i] == ',' || statement[i] == ')')
                {
                    var pattern = statement[i] == '(' ? Constants.EntityNamePattern : Constants.EntityDesignPattern;
                    token = AddToken(token, pattern, tokens);
                }

                token += statement[i];
            }

            AddToken(token, Constants.EntityDesignPattern, tokens);

            return tokens;
        }

        private static string AddToken(string token, string pattern, ICollection<string> result)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;

            if (!Regex.IsMatch(token, pattern))
            {
                throw new SyntacticErrorException(token);
            }

            result.Add(token);
            return string.Empty;
        }
    }
}