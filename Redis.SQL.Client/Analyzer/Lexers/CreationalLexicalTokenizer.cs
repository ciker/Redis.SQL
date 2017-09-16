using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

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

                if (statement.Substring(i).StartsWithKeyword(createKeyword))
                {
                    i += createKeyword.Length - 1;
                    tokens.AddLexicalToken(createKeyword, string.Empty);
                    continue;
                }

                if (statement[i] == '(' || statement[i] == ',' || statement[i] == ')')
                {
                    var pattern = statement[i] == '(' ? Constants.EntityNamePattern : Constants.EntityDesignPattern;
                    token = tokens.AddLexicalToken(token, pattern);
                    continue;
                }

                token += statement[i];
            }

            tokens.AddLexicalToken(token, Constants.EntityDesignPattern);

            return tokens;
        }
    }
}