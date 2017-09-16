using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class InsertionLexicalTokenizer : ILexer
    {
        public IEnumerable<string> Tokenize(string statement)
        {
            var tokens = new List<string>();
            var values = false;
            string token = string.Empty, insertKeyword = Keywords.Insert.ToString(), valuesKeyword = Keywords.Values.ToString();

            for (var i = 0; i < statement.Length; i++)
            {
                if (!values && statement[i] == ' ') continue;

                if (statement.Substring(i).StartsWithKeyword(valuesKeyword))
                {
                    i += insertKeyword.Length - 1;
                    token = tokens.AddLexicalToken(insertKeyword, string.Empty);
                    continue;
                }

                if (statement.Substring(i).StartsWithKeyword(valuesKeyword))
                {
                    values = true;
                    i += valuesKeyword.Length - 1;
                    token = tokens.AddLexicalToken(valuesKeyword, string.Empty);
                    continue;
                }

                if (statement[i] == '(' || statement[i] == ',' || statement[i] == ')')
                {
                    var pattern = values ? Constants.EntityNamePattern : Constants.PropertyValuePattern;
                    token = tokens.AddLexicalToken(token, pattern);
                    continue;
                }

                token += statement[i];
            }

            return tokens;
        }
    }
}