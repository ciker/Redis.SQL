using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class DeletionLexicalTokenizer : ILexer
    {
        public IList<string> Tokenize(string statement)
        {
            var tokens = new List<string>();

            string token = string.Empty, deleteKeyword = Keywords.Delete.ToString(), whereKeyword = Keywords.Where.ToString();

            for (var i = 0; i < statement.Length; i++)
            {
                if (statement[i] == ' ') continue;

                if (statement.Substring(i).StartsWithKeyword(deleteKeyword, ' '))
                {
                    i += deleteKeyword.Length - 1;
                    token = tokens.AddLexicalToken(deleteKeyword, string.Empty);
                    continue;
                }

                if (statement.Substring(i).StartsWithKeyword(whereKeyword, ' ', '('))
                {
                    tokens.AddLexicalToken(token, Constants.EntityNamePattern);
                    tokens.AddLexicalToken(whereKeyword, string.Empty);
                    var whereStatement = i + whereKeyword.Length < statement.Length? statement.Substring(i + whereKeyword.Length).Trim() : string.Empty;
                    token = tokens.AddLexicalToken(whereStatement, string.Empty);
                    break;
                }

                token += statement[i];
            }

            tokens.AddLexicalToken(token, Constants.EntityNamePattern);

            return tokens;
        }
    }
}