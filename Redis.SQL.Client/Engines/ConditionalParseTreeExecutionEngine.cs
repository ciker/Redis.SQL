using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redis.SQL.Client.Analyzer;
using Redis.SQL.Client.Enums;

namespace Redis.SQL.Client.Engines
{
    internal partial class RedisSqlQueryEngine
    {
        internal async Task<IEnumerable<string>> ExecuteTree(string entityName, BinaryTree<string> tree)
        {
            while (!tree.IsLeaf())
            {
                foreach (var node in tree)
                {
                    if (TryParseNode(node, out var _)) continue;

                    if (TryParseExpression(node, out var property, out var op, out var value))
                    {
                        PurgeNode(node, await ExecuteCondition(entityName, property, op, value));
                    }
                    else if (TryParseNode(node.Parent, out var parent) && (parent == WhereGrammar.Anding || parent == WhereGrammar.Oring))
                    {
                        var sibling = node.GetSibling();
                        if (TryParseNode(sibling, out var _) || TryParseExpression(sibling, out var _, out var _, out var _)) continue;
                        var nodeResults = string.IsNullOrEmpty(node.Value) ? new string[0] : node.Value.Split(',');
                        var siblingResults = string.IsNullOrEmpty(sibling.Value) ? new string[0] : sibling.Value.Split(',');
                        var result = string.Join(",", parent == WhereGrammar.Anding ? nodeResults.Intersect(siblingResults) : nodeResults.Union(siblingResults));
                        PurgeNode(node.Parent, result);
                    }
                }
            }

            return string.IsNullOrEmpty(tree.Value)? new string[0] : tree.Value.Split(',');
        }

        private static bool TryParseExpression(BinaryTree<string> node, out string property, out Operator op, out string value)
        {
            property = default(string);
            op = default(Operator);
            value = default(string);

            if (string.IsNullOrEmpty(node.Value)) return false;

            for (var i = 0; i < Operators.Length; i++)
            {
                string operation = Operators[i], prop = node.Value?.Split(new[] { operation }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
                
                if (string.Equals(prop, node.Value, StringComparison.OrdinalIgnoreCase) || (!string.IsNullOrEmpty(prop) && prop.Any(x => x == '\''))) continue;

                value = node.Value.Substring(node.Value.IndexOf(operation, StringComparison.OrdinalIgnoreCase) + operation.Length).Trim('\'');
                op = (Operator)Math.Pow(2D, i);
                property = prop;

                return true;
            }

            return false;
        }

        private static void PurgeNode(BinaryTree<string> node, string keys)
        {
            WhereGrammar[] excluded = { WhereGrammar.Anding, WhereGrammar.Oring };

            var parent = node.Parent;

            while (parent != null && excluded.All(x => !string.Equals(x.ToString(), parent.Value, StringComparison.CurrentCultureIgnoreCase)))
            {
                parent.LeftChild = null;
                parent.RightChild = null;
                parent.Value = keys;
                parent = parent.Parent;
            }
        }

        private static bool TryParseNode(BinaryTree<string> node, out WhereGrammar grammar) => Enum.TryParse(node.Value, true, out grammar);
    }
}
