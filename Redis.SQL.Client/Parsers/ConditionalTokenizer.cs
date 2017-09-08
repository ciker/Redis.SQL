using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Parsers
{
    internal class ConditionalTokenizer
    {
        private BinaryTree<string> _parseTree;

        internal ConditionalTokenizer()
        {
            _parseTree = new BinaryTree<string>();
        }

        internal BinaryTree<string> Tokenize(string condition)
        {
            var stringParam = false;
            string token = string.Empty, lastOperator = string.Empty;

            for (var i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '\'')
                {
                    token += '\'';
                    stringParam = !stringParam;
                    if (!stringParam)
                        token = AddToken(_parseTree, token);
                    continue;
                }

                if (stringParam)
                {
                    token += condition[i];
                    continue;
                }

                if (condition[i] == ' ') continue;

                if (condition[i] == '(')
                {
                    var child = new BinaryTree<string>();
                    _parseTree.AddChild(child);
                    _parseTree = child;
                    continue;
                }

                if (condition[i] == ')')
                {
                    if (!string.IsNullOrEmpty(token))
                        token = AddToken(_parseTree, token);

                    if (_parseTree.Parent != null)
                        _parseTree = _parseTree.Parent;

                    continue;
                }

                if (IsKeyword(condition.Substring(i), out var keyword))
                {
                    lastOperator = keyword;
                    token = AddToken(_parseTree, token);

                    if (!string.IsNullOrWhiteSpace(_parseTree.Value))
                        _parseTree = _parseTree.Parent ?? (_parseTree.Parent = new BinaryTree<string> { LeftChild = _parseTree });

                    _parseTree.SetValue(keyword);
                    i += keyword.Length - 1;
                    continue;
                }

                token += condition[i];
            }

            if (!string.IsNullOrWhiteSpace(token))
                _parseTree.Value = lastOperator;

            AddToken(_parseTree, token);

            return _parseTree = _parseTree.GetRoot();
        }

        private static string AddToken(BinaryTree<string> currentNode, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return string.Empty;
            currentNode.AddChild(new BinaryTree<string>(token));
            return string.Empty;
        }

        private static bool IsKeyword(string condition, out string keyword)
        {
            keyword = null;
            condition = condition.ToLower();
            string andKeyword = Keywords.And.ToString().ToLower(), orKeyword = Keywords.Or.ToString().ToLower();

            if (condition.StartsWith(andKeyword + " ") || condition.StartsWith(andKeyword + "("))
            {
                keyword = andKeyword;
                return true;
            }

            if (condition.StartsWith(orKeyword + " ") || condition.StartsWith(orKeyword + "("))
            {
                keyword = orKeyword;
                return true;
            }

            return false;
        }
    }
}
