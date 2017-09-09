namespace Redis.SQL.Client.Analyzer
{
    internal class GrammarRule
    {
        private string _symbol;

        internal string Symbol
        {
            get => _symbol;
            set => _symbol = value.ToUpper();
        }

        internal string Derivation { get; set; }
    }
}
