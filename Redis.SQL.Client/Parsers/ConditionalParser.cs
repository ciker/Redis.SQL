using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Engines;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Parsers
{
    public class ConditionalParser
    {
        private readonly char[] _trimFromClauses = {' ', '(', ')'};
        private readonly string[] _operations = {">=", "<=", ">", "<", "!=", "="};

        private bool Parse(string condition, ICollection<string> result, ICollection<string> operators)
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
                    string orString = Keywords.Or.ToString().ToLower(), andString = Keywords.And.ToString().ToLower();
                    if (condition.Substring(i).ToLower().StartsWith(orString + " "))
                    {
                        if (!string.IsNullOrWhiteSpace(clause)) result.Add(clause.Trim(_trimFromClauses));
                        operators.Add(orString);
                        return Parse(condition.Substring(i + 2), result, operators);
                    }
                    
                    if (condition.Substring(i).ToLower().StartsWith(andString + " "))
                    {
                        if (!string.IsNullOrWhiteSpace(clause)) result.Add(clause.Trim(_trimFromClauses));
                        operators.Add(andString);
                        return Parse(condition.Substring(i + 3), result, operators);
                    }
                }

                SetClause:
                clause += condition[i];
                innerCounter++;

                if (singleQuote) continue;

                if (openings > 0 && openings == closings) //Extract the inner clause from the brackets and parse the rest of the string
                {
                    return Parse(clause.Substring(1, innerCounter - 2), result, operators) && Parse(condition.Substring(i + 1), result, operators);
                }
            }

            result.Add(clause.Trim(_trimFromClauses));
            return true;
        }

        public async Task ParseCondition(string condition)
        {
            ICollection<string> clauses = new List<string>(), operators = new List<string>();
            Parse(condition, clauses, operators);

            while (clauses.Any())
            {
                var clause = clauses.First();

                for (var i = 0; i < _operations.Length; i++)
                {
                    var operation = _operations[i];
                    var property = clause.Split(new []{ operation }, StringSplitOptions.RemoveEmptyEntries).First().Trim();

                    if (!string.Equals(property, clause, StringComparison.OrdinalIgnoreCase) && property.All(x => x != '\''))
                    {
                        var value = clause.Substring(clause.IndexOf(operation, StringComparison.OrdinalIgnoreCase) + operation.Length).Trim('\'', ' ');
                        var x = await new RedisSqlQueryEngine().ExecuteCondition("user", property, (Operator)(Math.Pow(2D, i)), value);
                    }
                }


                


            }
        }
    }
}
