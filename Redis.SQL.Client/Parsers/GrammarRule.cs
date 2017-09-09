namespace Redis.SQL.Client.Parsers
{
    public class GrammarRule
    {
        private string _symbol;

        public string Symbol
        {
            get => _symbol;
            set => _symbol = value.ToUpper();
        }

        public string Derivation { get; set; }
    }
}
