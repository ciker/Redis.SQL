using System;
using System.Collections.Generic;
using System.Linq;
using Redis.SQL.Client.Analyzer.Interfaces;
using Redis.SQL.Client.Enums;
using Redis.SQL.Client.Exceptions;
using Redis.SQL.Client.Models;

namespace Redis.SQL.Client.Analyzer.Parsers
{
    internal class ProjectionalParser : ICustomizedParser
    {
        public BaseModel ParseTokens(IList<string> tokens)
        {
            var model = new ProjectionModel
            {
                ProjectedProperties = new List<string>()
            };

            bool fromKeywordParsed = false, whereKeywordParsed = false;

            tokens.RemoveAt(0);

            while (tokens.Count > 0)
            {
                var token = tokens[0];
                if (!fromKeywordParsed && string.Equals(token, Keywords.From.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (!model.ProjectAllProperties && !model.ProjectedProperties.Any())
                        throw new ParsingException("Projection Not Provided");

                    fromKeywordParsed = true;
                    tokens.RemoveAt(0);
                    continue;
                }

                if (fromKeywordParsed && !whereKeywordParsed && string.Equals(token, Keywords.Where.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(model.EntityName))
                        throw new ParsingException("Entity Name Not Provided");

                    whereKeywordParsed = true;
                    tokens.RemoveAt(0);

                    if (!tokens.Any())
                        throw new ParsingException("Where Condition Not Provided");

                    continue;
                }

                if (!fromKeywordParsed)
                {
                    if (token == "*")
                        model.ProjectAllProperties = true;
                    else
                    {
                        if (model.ProjectAllProperties)
                            throw new ParsingException("Cannot Project Properties Alongside the * Operator");

                        model.ProjectedProperties.Add(token.Split('.').Last());
                    }
                }

                if (fromKeywordParsed && !whereKeywordParsed)
                    model.EntityName = token;

                if (fromKeywordParsed && whereKeywordParsed)
                    model.Query = token;

                tokens.RemoveAt(0);
            }

            return model;
        }
    }
}