using System;
using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class ProjectionalLexicalTokenizer : ILexer
    {
        public IList<string> Tokenize(string statement)
        {
            var result = new List<string>();

            string token = string.Empty, fromKeyword = Keywords.From.ToString(), 
                whereKeyword = Keywords.Where.ToString(), selectKeyword = Keywords.Select.ToString();

            for (var i = 0; i < statement.Length; i++)
            {
                if (statement[i] == ' ') continue;

                if (statement.Substring(i).StartsWithKeyword(selectKeyword, ' ', '*'))
                {
                    i += selectKeyword.Length - 1;
                    result.AddLexicalToken(selectKeyword, string.Empty);
                    continue;
                }

                if (statement.Substring(i).StartsWithKeyword(fromKeyword, ' '))
                {
                    i += fromKeyword.Length - 1;
                    token = result.AddLexicalToken(token, Constants.ProjectionTokenPattern);
                    result.Add(fromKeyword.ToLower());
                    continue;
                }

                if (statement.Substring(i).StartsWithKeyword(whereKeyword, ' ', '(') || string.Equals(statement.Substring(i), whereKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    token = result.AddLexicalToken(token, Constants.EntityNamePattern);
                    result.Add(whereKeyword.ToLower());
                    if(i + whereKeyword.Length <= statement.Length - 1) result.Add(statement.Substring(i + whereKeyword.Length).Trim());
                    break;
                }

                if (statement[i] == ',')
                {
                    token = result.AddLexicalToken(token, Constants.ProjectionTokenPattern);
                    continue;
                }

                token += statement[i];
            }

            result.AddLexicalToken(token, string.Empty);
            return result;
        }
    }
}