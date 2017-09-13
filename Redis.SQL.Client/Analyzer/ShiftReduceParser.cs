using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Redis.SQL.Client.Analyzer
{
    internal class ShiftReduceParser
    {
        private readonly IEnumerable<GrammarRule> _grammar;

        private readonly IEnumerable<string> _excludedFromTree;

        internal ShiftReduceParser(Grammar grammar)
        {
            _excludedFromTree = grammar.ExcludedTokens ?? new string[0];
            _grammar = grammar.Rules;
        }

        private bool ParseTokens(IList<string> tokens, out BinaryTree<string> tree)
        {
            var parseStack = new Stack<KeyValuePair<string, BinaryTree<string>>>();
            
            while(tokens.Any())
            {
                ReduceFullStack(parseStack, false);
                Shift(parseStack, tokens);
            }

            ReduceFullStack(parseStack, false);
            ReduceFullStack(parseStack, true);

            tree = parseStack.Pop().Value;
            return !parseStack.Any() && tree.Value == _grammar.First().Symbol;
        }

        private static void Shift(Stack<KeyValuePair<string, BinaryTree<string>>> parseStack, IList<string> tokens)
        {
            var token = tokens[0];
            parseStack.Push(new KeyValuePair<string, BinaryTree<string>>(token, new BinaryTree<string>(token)));
            tokens.RemoveAt(0);
        }

        private void ReduceFullStack(Stack<KeyValuePair<string, BinaryTree<string>>> parseStack, bool reduceRoot)
        {
            while (Reduce(parseStack, reduceRoot))
            {
                //Do Nothing, just reduce the stack till the end
            }
        }

        private bool Reduce(Stack<KeyValuePair<string, BinaryTree<string>>> parseStack, bool reduceRoot)
        {
            IList<KeyValuePair<string, BinaryTree<string>>> stackElements = 
                new List<KeyValuePair<string, BinaryTree<string>>>();

            while (parseStack.Count > 0)
            {
                var peek = parseStack.Pop();
                stackElements.Insert(0, peek);
                var stringElements = string.Join(string.Empty, stackElements.Select(x => x.Key)); 
                if (CanReduce(stringElements, reduceRoot, out var grammarRule))
                {
                    var replacement = grammarRule.Symbol;
                    var tree = new BinaryTree<string>(grammarRule.Symbol);

                    foreach (var element in stackElements)
                    {
                        if(_excludedFromTree.All(x => element.Key.Trim() != x))
                            tree.AddChild(element.Value);
                    }

                    parseStack.Push(new KeyValuePair<string, BinaryTree<string>>(replacement, tree));
                    return true;
                }
            }

            foreach (var item in stackElements)
            {
                parseStack.Push(item);
            }

            return false;
        }

        private bool CanReduce(string input, bool reduceRoot, out GrammarRule rule)
        {
            var rules = reduceRoot? _grammar : _grammar.Skip(1);
            rule = null;
            foreach (var grammarRule in rules)
            {
                if (Regex.IsMatch(input, grammarRule.Derivation))
                {
                    rule = grammarRule;
                    return true;
                }
            }
            return false;
        }

        internal BinaryTree<string> ParseCondition(IEnumerable<string> tokens)
        {
            if (!ParseTokens(tokens.ToList(), out var tree))
            {
                throw new ParsingException();
            }
            return tree;
        }
    }
}