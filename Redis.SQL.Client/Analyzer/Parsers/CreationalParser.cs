using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Parsers
{
    internal class CreationalParser : ICustomizedParser
    {
        public BaseModel ParseTokens(IList<string> tokens)
        {
            tokens.RemoveAt(0);
            var result = new CreationModel
            {
                Properties = new Dictionary<string, string>()
            };

            var entityNameParsed = false;
            while(tokens.Count > 0)
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
                else
                {
                    if (Regex.IsMatch(token, Constants.EntityDesignPattern))
                    {
                        var split = token.Split(':');
                        var propertyName = split[0];
                        var propertyType = split[1];

                        if (Enum.TryParse(propertyType, true, out TypeNames _))
                        {
                            result.Properties.Add(propertyName, propertyType.ToLower());
                        }
                        else
                        {
                            throw new UnsupportedTypeException(propertyType);
                        }
                    }
                    else
                    {
                        throw new ParsingException();
                    }
                }

                tokens.RemoveAt(0);
            }

            return result;
        }
    }
}