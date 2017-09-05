using System.Collections.Generic;

namespace Redis.SQL.Client.Parsers
{
    public class ConditionalParser
    {
        private readonly char[] _trimFromClauses = {' ', '(', ')'};

        public bool ParseCondition(string condition, ICollection<string> result, ICollection<string> operators)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;

            int openings = 0, closings = 0, innerCounter = 0;

            var singleQuote = false;

            var clause = string.Empty;

            for (var i = 0; i < condition.Length; i++) //Iterate over the whole string
            {
                switch (condition[i])
                {
                    case '(': if(!singleQuote) openings++; break;
                    case ')': if(!singleQuote) closings++; break;
                    case '\'': singleQuote = !singleQuote; break;
                }

                if (singleQuote) goto SetClause; //Dont parse special keywords until the single quote is closed

                if (openings == closings && (i == 0 || condition[i - 1] == ' '))
                {
                    if (condition.Substring(i).ToLower().StartsWith("or "))
                    {
                        if (!string.IsNullOrWhiteSpace(clause)) result.Add(clause.Trim(_trimFromClauses));
                        operators.Add("or");
                        return ParseCondition(condition.Substring(i + 2), result, operators);
                    }
                    
                    if (condition.Substring(i).ToLower().StartsWith("and "))
                    {
                        if (!string.IsNullOrWhiteSpace(clause)) result.Add(clause.Trim(_trimFromClauses));
                        operators.Add("and");
                        return ParseCondition(condition.Substring(i + 3), result, operators);
                    }
                }

                SetClause:
                clause += condition[i];
                innerCounter++;

                if (singleQuote) continue;

                if (openings > 0 && openings == closings) //Extract the inner clause from the brackets and parse the rest of the string
                {
                    return ParseCondition(clause.Substring(1, innerCounter - 2), result, operators) && ParseCondition(condition.Substring(i + 1), result, operators);
                }
            }

            result.Add(clause.Trim(_trimFromClauses));
            return true;
        }
    }
}
