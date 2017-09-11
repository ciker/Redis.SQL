using System.Collections;
using System.Collections.Generic;

namespace Redis.SQL.Client.Analyzer
{
    internal class BinaryTree<T> : IEnumerable<BinaryTree<T>>
    {
        internal BinaryTree<T> LeftChild { get; set; }

        internal BinaryTree<T> RightChild { get; set; }

        internal BinaryTree<T> Parent { get; set; }

        internal T Value { get; set; }

        internal BinaryTree()
        {

        }

        internal BinaryTree(T value)
        {
            Value = value;
        }

        internal BinaryTree(BinaryTree<T> parent)
        {
            Parent = parent;
        }

        internal BinaryTree<T> SetValue(T value)
        {
            Value = value;
            return this;
        }
        
        internal BinaryTree<T> AddChild(BinaryTree<T> child)
        {
            child.Parent = this;

            if (LeftChild == null)
            {
                LeftChild = child;
            }
            else if (RightChild == null)
            {
                RightChild = child;
            }
            else
            {
                Parent = new BinaryTree<T>
                {
                    LeftChild = this,
                    RightChild = child
                };
            }

            return this;
        }

        internal BinaryTree<T> GetRoot()
        {
            var pointer = this;
            while (!pointer.IsRoot())
                pointer = pointer.Parent;
            return pointer;
        }

        internal BinaryTree<T> GetSibling()
        {
            return Parent.LeftChild == this ? Parent.RightChild : Parent.LeftChild;
        }

        internal bool IsLeaf() => LeftChild == null && RightChild == null;

        internal bool IsRoot() => Parent == null;

        public IEnumerator<BinaryTree<T>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<BinaryTree<T>>
        {
            private readonly Stack<BinaryTree<T>> _searchStack = new Stack<BinaryTree<T>>();

            public Enumerator(BinaryTree<T> tree)
            {
                Current = tree;
                _searchStack.Push(Current);
            }

            public bool MoveNext()
            {
                if (_searchStack.Count == 0) return false;

                Current = _searchStack.Pop();

                if (Current.IsLeaf()) return true;

                if (Current.RightChild != null)
                {
                    _searchStack.Push(Current.RightChild);
                }

                _searchStack.Push(Current.LeftChild);

                return true;
            }

            public void Reset()
            {
            }

            public BinaryTree<T> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
