using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Analyzer
{
    internal class Grammar
    {
        internal GrammarRule[] Rules { get; private set; }

        internal string[] ExcludedTokens { get; private set; }

        internal Grammar(GrammarType type)
        {
            switch (type)
            {
                case GrammarType.Where:
                    SetWhereGrammar();
                    break;
                case GrammarType.Expression:
                    SetExpressionGrammar();
                    break;
                case GrammarType.Select:
                    break;
            }
        }

        private void SetExpressionGrammar()
        {
            string ExpressionToString(ExpressionGrammar arg) => arg.ToString().ToUpper();

            string root = ExpressionToString(ExpressionGrammar.Root),
                exp = ExpressionToString(ExpressionGrammar.Exp),
                num = ExpressionToString(ExpressionGrammar.Num),
                str = ExpressionToString(ExpressionGrammar.Str),
                boolean = ExpressionToString(ExpressionGrammar.Bool),
                variable = ExpressionToString(ExpressionGrammar.Var);
            
            Rules = new[]
            {
                new GrammarRule {Symbol = root, Derivation = exp},
                new GrammarRule {Symbol = exp, Derivation = variable + "=" + str},
                new GrammarRule {Symbol = exp, Derivation = variable + ">=" + str},
                new GrammarRule {Symbol = exp, Derivation = variable + ">" + str},
                new GrammarRule {Symbol = exp, Derivation = variable + "<=" + str},
                new GrammarRule {Symbol = exp, Derivation = variable + "<" + str},
                new GrammarRule {Symbol = exp, Derivation = variable + "=" + num},
                new GrammarRule {Symbol = exp, Derivation = variable + ">=" + num},
                new GrammarRule {Symbol = exp, Derivation = variable + ">" + num},
                new GrammarRule {Symbol = exp, Derivation = variable + "<=" + num},
                new GrammarRule {Symbol = exp, Derivation = variable + "<" + num},
                new GrammarRule {Symbol = exp, Derivation = variable + "=" + boolean},
                new GrammarRule {Symbol = num, Derivation = "[0-9]+"},
                new GrammarRule {Symbol = str, Derivation = @"\'.*\'"},
                new GrammarRule {Symbol = boolean, Derivation = "[tT][rR][uU][eE]"},
                new GrammarRule {Symbol = boolean, Derivation = "[fF][aA][lL][sS][eE]"},
                new GrammarRule {Symbol = variable, Derivation = @"([\w-]+|[\w-]+.[\w-]+)"},
            };
        }

        private void SetWhereGrammar()
        {
            string WhereToString(WhereGrammar arg) => arg.ToString().ToUpper();

            string root = WhereToString(WhereGrammar.Root),
                anding = WhereToString(WhereGrammar.Anding),
                oring = WhereToString(WhereGrammar.Oring),
                operand = WhereToString(WhereGrammar.Operand),
                expression = WhereToString(WhereGrammar.Expression);

            Rules = new[]
            {
                new GrammarRule {Symbol = root, Derivation = operand},
                new GrammarRule {Symbol = anding, Derivation = operand + " and " + operand},
                new GrammarRule {Symbol = oring, Derivation = operand + " or " + operand},
                new GrammarRule {Symbol = operand, Derivation = anding},
                new GrammarRule {Symbol = operand, Derivation = oring},
                new GrammarRule {Symbol = operand, Derivation = "[(]" + operand + "[)]"},
                new GrammarRule {Symbol = operand, Derivation = expression},
                new GrammarRule {Symbol = operand,Derivation = @"([\w-]+|[\w-]+.[\w-]+)(!=|=|>=|>|<|<=)+([\d.-]+|'[a-zA-Z0-9\s]+'|[tT][rR][uU][eE]|[fF][aA][lL][sS][eE])+"}
            };

            ExcludedTokens = new[] { "(", ")", "and", "or" };
        }
    }
}