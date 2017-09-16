using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class ProjectionalLexicalTokenizer : ILexer
    {
        public IEnumerable<string> Tokenize(string statement)
        {
            var result = new List<string>();

            string token = string.Empty, fromKeyword = Keywords.From.ToString(), whereKeyword = Keywords.Where.ToString(), selectKeyword = Keywords.Select.ToString();
            for (var i = 0; i < statement.Length; i++)
            {
                if (statement[i] == ' ') continue;

                if (statement.Substring(i).StartsWith(selectKeyword + " ", StringComparison.OrdinalIgnoreCase))
                {
                    i += selectKeyword.Length - 1;
                    result.Add(selectKeyword.ToLower());
                    continue;
                }

                if (statement.Substring(i).StartsWith(fromKeyword + " ", StringComparison.OrdinalIgnoreCase))
                {
                    i += fromKeyword.Length - 1;
                    token = AddToken(token, Constants.ProjectionTokenPattern, result);
                    result.Add(fromKeyword.ToLower());
                    continue;
                }

                if (statement.Substring(i).StartsWith(whereKeyword + " ", StringComparison.OrdinalIgnoreCase) || statement.Substring(i).StartsWith(whereKeyword + "(", StringComparison.OrdinalIgnoreCase) || string.Equals(statement.Substring(i), whereKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    token = AddToken(token, Constants.EntityNamePattern, result);
                    result.Add(whereKeyword.ToLower());
                    if(i + whereKeyword.Length <= statement.Length - 1) result.Add(statement.Substring(i + whereKeyword.Length).Trim());
                    break;
                }

                if (statement[i] == ',')
                {
                    token = AddToken(token, Constants.ProjectionTokenPattern, result);
                    continue;
                }

                token += statement[i];
            }

            AddToken(token, string.Empty, result);
            return result;
        }

        private static string AddToken(string token, string pattern, ICollection<string> collection)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;

            if (!string.IsNullOrEmpty(pattern) && !Regex.IsMatch(token, pattern))
            {
                throw new SyntacticErrorException(token);
            }

            collection.Add(token);
            return string.Empty;
        }
    }
}