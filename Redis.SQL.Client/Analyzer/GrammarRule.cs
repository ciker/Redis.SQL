namespace Redis.SQL.Client.Analyzer
{
    internal class GrammarRule
    {
        private string _symbol;

        private string _derivation;

        internal string Symbol
        {
            get => _symbol;
            set => _symbol = value.ToUpper();
        }

        internal string Derivation
        {
            get => _derivation;
            set => _derivation = "^" + value + "$";
        }
    }
}
