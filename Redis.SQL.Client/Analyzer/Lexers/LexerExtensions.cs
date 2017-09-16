using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Exceptions;

namespace Redis.SQL.Client.Analyzer.Lexers
{
    internal static class LexerExtensions
    {
        internal static string AddLexicalToken(this ICollection<string> src, string token, string pattern)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;

            if (!string.IsNullOrEmpty(pattern) && !Regex.IsMatch(token.Trim(), pattern))
            {
                throw new SyntacticErrorException(token);
            }

            src.Add(token);
            return string.Empty;
        }

        internal static bool StartsWithKeyword(this string statement, string keyword)
        {
            return statement.StartsWith($"{keyword} ", StringComparison.OrdinalIgnoreCase);
        }
    }
}
