using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Engines;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Parsers
{
    public class ConditionalParser
    {
        private readonly RedisSqlQueryEngine _queryEngine;

        public ConditionalParser()
        {
            _queryEngine = new RedisSqlQueryEngine();
        }

        private readonly string[] _operations = {">=", "<=", ">", "<", "!=", "="};

        private static bool Parse(string condition, ICollection<string> clauses)
        {
            if (string.IsNullOrWhiteSpace(condition)) return true;

            condition = condition.Trim();

            int openings = 0, closings = 0;

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

                bool orOperator = IsOrOperator(condition.Substring(i)), andOperator = IsAndOperator(condition.Substring(i));

                if (openings == closings && (orOperator || andOperator))
                {
                    if (!string.IsNullOrWhiteSpace(clause))
                    {
                        clauses.Add(clause.Trim());
                    }

                    if (!(IsAndOperator(condition) || IsOrOperator(condition)))
                    {
                        clauses.Add("(" + condition + ")");
                    }
                    return Parse(condition.Substring(andOperator ? i + 3 : i + 2).Trim(), clauses);
                }


            SetClause:
                clause += condition[i];

                if (singleQuote) continue;

                if (openings > 0 && openings == closings)
                {
                    if (condition.Contains(" " + Keywords.And.ToString().ToLower() + " ") || condition.Contains(" " + Keywords.Or.ToString().ToLower() + " "))
                    {
                        clauses.Add("(" + condition + ")");
                    }
                    else
                    {
                        clauses.Add(condition.Substring(clause.IndexOf('(') + 1, clause.LastIndexOf(')') - 1));
                    }
                    if (clause.Contains(" " + Keywords.And.ToString().ToLower() + " ") || clause.Contains(" " + Keywords.Or.ToString().ToLower() + " "))
                    {
                        clauses.Add("(" + clause + ")");
                    }
                    return Parse(clause.Substring(clause.IndexOf('(') + 1, clause.LastIndexOf(')') - 1), clauses) && Parse(condition.Substring(i + 1), clauses);
                }
            }

            clauses.Add(clause);
            return true;
        }

        private static IList<string> OrderClauses(IEnumerable<string> clauses)
        {
            return clauses.Select(x => x.Trim()).OrderBy(x => x.Split(new[] { Keywords.And.ToString().ToLower(), Keywords.Or.ToString().ToLower() }, StringSplitOptions.RemoveEmptyEntries).Length).ToList();
        }

        private static IList<string> FilterDuplicateClauses(IList<string> clauses)
        {
            clauses = clauses.Distinct().ToList();

            var duplicates = clauses.Where(clause => clauses.Any(x => string.Equals("(" + x + ")", clause, StringComparison.OrdinalIgnoreCase))).ToList();

            duplicates.ForEach(x => clauses.Remove(x));

            return clauses;
        }

        private static string RemoveWhiteSpacesFromCondition(string condition)
        {
            var stringParam = false;
            var result = string.Empty;
            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    result += condition[i];
                    stringParam = !stringParam;
                    continue;
                }

                if (stringParam)
                {
                    result += condition[i];
                    continue;
                }

                if (condition[i] == ' ') continue;

                if (IsAndOperator(condition.Substring(i)))
                {
                    result += " " + Keywords.And.ToString().ToLower() + " ";
                    i += 2;
                    continue;
                }

                if (IsOrOperator(condition.Substring(i)))
                {
                    result += " " + Keywords.Or.ToString().ToLower() + " ";
                    i++;
                    continue;
                }

                result += condition[i];
            }

            return result;
        }

        private static bool IsOrOperator(string condition)
        {
            condition = condition.ToLower();
            var orString = Keywords.Or.ToString().ToLower();
            return condition.StartsWith(orString + " ") || condition.StartsWith(orString + "(");
        }

        private static bool IsAndOperator(string condition)
        {
            condition = condition.ToLower();
            var andString = Keywords.And.ToString().ToLower();
            return condition.StartsWith(andString + " ") || condition.StartsWith(andString + "(");
        }

        public async Task ParseCondition(string entityName, string condition)
        {
            IList<string> clauses = new List<string>();

            var watch = new Stopwatch();


            Parse(RemoveWhiteSpacesFromCondition(condition), clauses);
            clauses = FilterDuplicateClauses(OrderClauses(clauses));
            
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
                        var keys = await _queryEngine.ExecuteCondition(entityName, property, (Operator)Math.Pow(2D, i), value);
                    }
                }
            }
        }
    }
}
