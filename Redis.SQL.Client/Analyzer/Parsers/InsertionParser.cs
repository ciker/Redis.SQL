using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Parsers
{
    internal class InsertionParser : ICustomizedParser
    {
        public BaseModel ParseTokens(IList<string> tokens)
        {
            var result = new InsertionModel
            {
                PropertyValues = new Dictionary<string, string>()
            };

            tokens.RemoveAt(0);
            var valuesKeyword = Keywords.Values.ToString();
            bool entityNameParsed = false, valuesParsed = false;
            var properties = new List<string>();
            var values = new List<string>();

            while (tokens.Count > 0)
            {
                var token = tokens[0];
                
                if (!entityNameParsed)
                {
                    if (Regex.IsMatch(token, Constants.EntityNamePattern))
                    {
                        result.EntityName = token;
                        entityNameParsed = true;
                    }
                    else
                    {
                        throw new ParsingException();
                    }
                } 
                else if (string.Equals(token, valuesKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    valuesParsed = true;
                }
                else if (!valuesParsed)
                {
                    var property = token[0].ToString().ToUpper() + token.Substring(1);
                    properties.Add(property.Trim());
                }
                else
                {
                    values.Add(token.Trim('\''));
                }

                tokens.RemoveAt(0);
            }

            if (values.Count != properties.Count)
            {
                throw new ParsingException("The provided number of values does not match the provided number of properties");
            }

            for (var i = 0; i < values.Count; i++)
            {
                result.PropertyValues.Add(properties[i], values[i]);
            }

            return result;
        }
    }
}