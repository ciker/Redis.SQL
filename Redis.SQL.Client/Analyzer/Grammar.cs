using System;
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
                    Rules = new []
                    {
                        new GrammarRule {Symbol = "ROOT", Derivation = "^OPERAND$"},
                        new GrammarRule {Symbol = "ANDING", Derivation = "^OPERAND and OPERAND$"},
                        new GrammarRule {Symbol = "ORING", Derivation = "^OPERAND or OPERAND$"},
                        new GrammarRule {Symbol = "OPERAND", Derivation = "^ANDING$"},
                        new GrammarRule {Symbol = "OPERAND", Derivation = "^ORING$"},
                        new GrammarRule {Symbol = "OPERAND", Derivation = "^[(]OPERAND[)]$"},
                        new GrammarRule {Symbol = "OPERAND", Derivation = "^EXPRESSION$"},
                        new GrammarRule {Symbol = "EXPRESSION",Derivation = @"^([\w-]+|[\w-]+.[\w-]+)(!=|=|>=|>|<|<=)+([\d.-]+|'[a-zA-Z0-9\s]+'|[tT]rue|[fF]alse)+$"}
                    };
                    ExcludedTokens = new [] {"(", ")", "and", "or"};
                    break;

                case GrammarType.Select:
                    break;
            }
        }
    }
}
