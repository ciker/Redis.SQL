using System.Collections.Generic;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal class UpdateLexicalTokenizer : ILexer
    {
        public IList<string> Tokenize(string statement)
        {
            var tokens = new List<string>();

            string updateKeyword = Keywords.Update.ToString(), setKeyword = Keywords.Set.ToString(), whereKeyword = Keywords.Where.ToString(), token = string.Empty;

            bool setKeywordTokenized = false, stringToken = false;

            for (var i = 0; i < statement.Length; i++)
            {
                if (statement[i] == '\'') stringToken = !stringToken;

                if(statement[i] == ' ' && !setKeywordTokenized) continue;

                if (statement.Substring(i).StartsWithKeyword(updateKeyword, ' ') && tokens.Count == 0)
                {
                    token = tokens.AddLexicalToken(updateKeyword, string.Empty);
                    i += updateKeyword.Length;
                }

                if (statement.Substring(i).StartsWithKeyword(setKeyword, ' ') && !setKeywordTokenized)
                {
                    token = tokens.AddLexicalToken(token?.Trim(), Constants.EntityNamePattern);
                    tokens.AddLexicalToken(setKeyword, string.Empty);
                    i += setKeyword.Length;
                    setKeywordTokenized = true;
                }

                if (setKeywordTokenized && !stringToken && statement[i] == ',')
                {
                    token = tokens.AddLexicalToken(token?.Trim(), Constants.WhereClauseTokenPattern);
                }

                if (setKeywordTokenized && statement.Substring(i).StartsWithKeyword(whereKeyword, ' ', '('))
                {
                    token = tokens.AddLexicalToken(token?.Trim(), Constants.WhereClauseTokenPattern);
                    tokens.AddLexicalToken(whereKeyword, string.Empty);
                    tokens.AddLexicalToken(statement.Substring(i), string.Empty);
                    break;
                }

                token += statement[i];
            }

            tokens.AddLexicalToken(token, string.Empty);

            return tokens;
        }
    }
}