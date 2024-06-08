﻿using System;
using System.Collections.Generic;
using DStruct.List;

namespace DStruct.Tree.avl
{
    public class AvlTree<TKey, TValue> : ISearchTree<TKey, TValue> where TKey : IComparable
    {
        private Node<TKey, TValue> _root;

        public int Size { private set; get; }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Put(key, value);
        }

        public void Put(TKey key, TValue val)
        {
            var pair = new KeyValuePair<TKey, TValue>(key, val);
            if (_root == null)
            {
                _root = new Node<TKey, TValue>(pair);
                Size++;
            }
            else
            {
                var newNode = Put(_root, pair);
                if (newNode != null)
                {
                    Size++;
                    AdjustHeights(newNode);
                    var balances = new Stack<int>();
                    var imbalanced = FindImbalanced(newNode.Parent, balances);
                    if (imbalanced != null)
                    {
                        Rotate(imbalanced, balances.Pop(), balances.Pop());
                    }
                }
            }
        }

        public TValue Get(TKey key)
        {
            EmptyCheck();
            var node = Get(_root, key);
            return node != null ? node.Val : default;
        }

        public bool Remove(TKey key)
        {
            EmptyCheck();
            var node = Get(_root, key);
            if (node != null)
            {
                Size--;
                var successor = Remove(node);
                AdjustHeights(node);
                var balances = new Stack<int>();
                var imbalanced = FindImbalanced(successor, balances);
                if (imbalanced != null)
                {
                    var pBalance = balances.Pop();
                    var middle = pBalance > 0 ? imbalanced.Right : imbalanced.Left;
                    Rotate(imbalanced, pBalance, CalcBalance(middle));
                }
            }

            return node != null;
        }

        public bool Contains(TKey key)
        {
            return Get(_root, key) != null;
        }

        public void Clear()
        {
            _root = null;
            Size = 0;
        }

        public bool IsEmpty()
        {
            return Size == 0;
        }

        public IEnumerable<TKey> Keys()
        {
            var list = new ArrayList<Node<TKey, TValue>>(Size);
            InOrder(_root, list);
            for (var i = 0; i < list.Size; i++)
            {
                yield return list[i].Key;
            }
        }

        public IEnumerable<TValue> Elements()
        {
            var list = new ArrayList<Node<TKey, TValue>>(Size);
            InOrder(_root, list);
            for (var i = 0; i < list.Size; i++)
            {
                yield return list[i].Val;
            }
        }

        public IEnumerable<TValue> RangeSearch(TKey lower, TKey upper)
        {
            var list = new ArrayList<KeyValuePair<TKey, TValue>>(Size);
            RangeSearch(_root, lower, upper, list);
            for (var i = 0; i < list.Size; i++)
            {
                yield return list[i].Value;
            }
        }

        public int Number(TKey key)
        {
            var node = Get(_root, key);
            return node != null ? Number(node) : 0;
        }

        public int Rank(TKey key)
        {
            var number = Rank(_root, key);
            return LessThan(_root.Key, key) ? ++number : number;
        }

        public TValue Min()
        {
            EmptyCheck();
            return Min(_root).Val;
        }

        public TValue Max()
        {
            EmptyCheck();
            return Max(_root).Val;
        }

        public TKey Select(int rank)
        {
            var selected = Select(_root, rank);
            return selected != null ? selected.Key : default;
        }

        private bool LessThan(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) == -1;
        }

        private bool GreaterThan(TKey key1, TKey key2)
        {
            return key1.CompareTo(key2) == 1;
        }

        private bool LessOrEqual(TKey key1, TKey key2)
        {
            return !GreaterThan(key1, key2);
        }

        private bool GreaterOrEqual(TKey key1, TKey key2)
        {
            return !LessThan(key1, key2);
        }

        private int Max(int val1, int val2)
        {
            return val1 > val2 ? val1 : val2;
        }

        private int Height(Node<TKey, TValue> node)
        {
            return node == null ? 0 : node.Height;
        }

        private int CalcBalance(Node<TKey, TValue> node)
        {
            return Height(node.Right) - Height(node.Left);
        }

        private bool ImBalanced(int balance)
        {
            return balance >= 2 || balance <= -2;
        }

        private void AdjustHeights(Node<TKey, TValue> node)
        {
            AdjustHeight(node);
            if (node.Parent != null)
            {
                AdjustHeights(node.Parent);
            }
        }

        private void AdjustHeight(Node<TKey, TValue> node)
        {
            node.Height = Max(Height(node.Left), Height(node.Right)) + 1;
        }

        private Node<TKey, TValue> FindImbalanced(Node<TKey, TValue> node, Stack<int> balances)
        {
            if (node != null)
            {
                var balance = CalcBalance(node);
                balances.Push(balance);
                return ImBalanced(balance) ? node : FindImbalanced(node.Parent, balances);
            }
            else
            {
                return null;
            }
        }

        private void Rotate(Node<TKey, TValue> imbalanced, int pBalance, int mBalance)
        {
            if (pBalance > 0)
            {
                if (mBalance > 0)
                {
                    LeftRotation(imbalanced.Right);
                }
                else
                {
                    RightLeftRotation(imbalanced.Right);
                }
            }
            else
            {
                if (mBalance < 0)
                {
                    RightRotation(imbalanced.Left);
                }
                else
                {
                    LeftRightRotation(imbalanced.Left);
                }
            }

            AdjustHeights(imbalanced);
        }

        private void SetRotationParent(Node<TKey, TValue> parentParent, Node<TKey, TValue> parent, Node<TKey, TValue> middle)
        {
            middle.Parent = parentParent;
            if (parentParent != null)
            {
                if (parentParent.Right == parent)
                {
                    parentParent.Right = middle;
                }
                else
                {
                    parentParent.Left = middle;
                }
            }
            else
            {
                _root = middle;
            }
        }

        private void LeftRotation(Node<TKey, TValue> middle)
        {
            var parent = middle.Parent;
            var parentParent = parent.Parent;
            var left = middle.Left;
            middle.Left = parent;
            parent.Parent = middle;
            parent.Right = left;
            if (left != null)
            {
                left.Parent = parent;
            }

            SetRotationParent(parentParent, parent, middle);
        }

        private void RightRotation(Node<TKey, TValue> middle)
        {
            var parent = middle.Parent;
            var parentParent = parent.Parent;
            var right = middle.Right;
            middle.Right = parent;
            parent.Parent = middle;
            parent.Left = right;
            if (right != null)
            {
                right.Parent = parent;
            }

            SetRotationParent(parentParent, parent, middle);
        }

        private void LeftRightRotation(Node<TKey, TValue> middle)
        {
            var child = middle.Right;
            var parent = middle.Parent;
            var childLeft = child.Left;
            child.Left = middle;
            child.Parent = parent;
            parent.Left = child;
            middle.Parent = child;
            middle.Right = childLeft;
            if (childLeft != null)
            {
                childLeft.Parent = middle;
            }

            RightRotation(child);
            AdjustHeight(middle);
        }

        private void RightLeftRotation(Node<TKey, TValue> middle)
        {
            var child = middle.Left;
            var parent = middle.Parent;
            var childRight = child.Right;
            child.Right = middle;
            child.Parent = parent;
            parent.Right = child;
            middle.Parent = child;
            middle.Left = childRight;
            if (childRight != null)
            {
                childRight.Parent = middle;
            }

            LeftRotation(child);
            AdjustHeight(middle);
        }

        private Node<TKey, TValue> Put(Node<TKey, TValue> tree, KeyValuePair<TKey, TValue> pair)
        {
            if (LessThan(pair.Key, tree.Key))
            {
                if (tree.Left == null)
                {
                    var newNode = new Node<TKey, TValue>(pair);
                    tree.Left = newNode;
                    newNode.Parent = tree;
                    return newNode;
                }
                else
                {
                    return Put(tree.Left, pair);
                }
            }
            else if (GreaterThan(pair.Key, tree.Key))
            {
                if (tree.Right == null)
                {
                    var newNode = new Node<TKey, TValue>(pair);
                    tree.Right = newNode;
                    newNode.Parent = tree;
                    return newNode;
                }
                else
                {
                    return Put(tree.Right, pair);
                }
            }
            else
            {
                tree.Data = pair;
                return null;
            }
        }

        private Node<TKey, TValue> Get(Node<TKey, TValue> tree, TKey key)
        {
            if (tree == null)
            {
                return null;
            } 
            else if (LessThan(key, tree.Key))
            {
                return tree.Left != null ? Get(tree.Left, key) : null;
            }
            else if (GreaterThan(key, tree.Key))
            {
                return tree.Right != null ? Get(tree.Right, key) : null;
            }
            else
            {
                return tree;
            }
        }

        private Node<TKey, TValue> Remove(Node<TKey, TValue> node)
        {
            var hasLeft = node.Left != null;
            var hasRight = node.Right != null;
            if (hasRight && hasLeft)
            {
                var successor = Min(node.Right);
                Move(successor, node);
                return Remove(successor);
            }
            else
            {
                if (hasRight)
                {
                    Replace(node, node.Right);
                    return node.Right;
                }
                else if (hasLeft)
                {
                    Replace(node, node.Left);
                    return node.Left;
                }
                else
                {
                    Replace(node, null);
                    return node.Parent;
                }
            }
        }

        private void Replace(Node<TKey, TValue> node, Node<TKey, TValue> newNode)
        {
            if (node.Parent == null)
            {
                _root = newNode;
                return;
            }

            if (node.Parent.Left == node)
            {
                node.Parent.Left = newNode;
            }
            else
            {
                node.Parent.Right = newNode;
            }

            if (newNode != null)
            {
                newNode.Parent = node.Parent;
            }
        }

        private Node<TKey, TValue> Min(Node<TKey, TValue> tree)
        {
            while (tree.Left != null)
            {
                tree = tree.Left;
            }

            return tree;
        }

        private Node<TKey, TValue> Max(Node<TKey, TValue> tree)
        {
            while (tree.Right != null)
            {
                tree = tree.Right;
            }

            return tree;
        }

        private void Move(Node<TKey, TValue> from, Node<TKey, TValue> to)
        {
            to.Data = from.Data;
        }

        private void InOrder(Node<TKey, TValue> node, ArrayList<Node<TKey, TValue>> list)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left != null)
            {
                InOrder(node.Left, list);
            }

            list.PushBack(node);
            if (node.Right != null)
            {
                InOrder(node.Right, list);
            }
        }

        private void RangeSearch(Node<TKey, TValue> node, TKey lower, TKey upper, ArrayList<KeyValuePair<TKey, TValue>> list)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left != null && GreaterOrEqual(node.Key, lower))
            {
                RangeSearch(node.Left, lower, upper, list);
            }

            if (LessOrEqual(node.Key, upper) && GreaterOrEqual(node.Key, lower))
            {
                list.PushBack(node.Data);
            }

            if (node.Right != null && LessOrEqual(node.Key, upper))
            {
                RangeSearch(node.Right, lower, upper, list);
            }
        }

        private int Number(Node<TKey, TValue> node)
        {
            return (node.Right != null ? Number(node.Right) + 1 : 0) +
                   (node.Left != null ? Number(node.Left) + 1 : 0);
        }

        private int Rank(Node<TKey, TValue> node, TKey key)
        {
            return (node.Right != null && LessThan(node.Key, key)
                       ? Rank(node.Right, key) + (LessThan(node.Right.Key, key) ? 1 : 0)
                       : 0) +
                   (node.Left != null ? Rank(node.Left, key) + (LessThan(node.Left.Key, key) ? 1 : 0) : 0);
        }

        private Node<TKey, TValue> Select(Node<TKey, TValue> node, int rank)
        {
            if (Rank(node.Key) == rank)
            {
                return node;
            }

            var left = node.Left != null ? Select(node.Left, rank) : null;
            var right = node.Right != null ? Select(node.Right, rank) : null;
            return left != null ? left : right;
        }

        private void EmptyCheck()
        {
            if (Size == 0)
            {
                throw new EmptyTreeException();
            }
        }

        public void PrintConsole()
        {
            if (_root == null)
            {
                Console.WriteLine(" | Empty");
                return;
            }

            PrintConsole(_root);
            Console.WriteLine();
        }

        private static void PrintConsole(Node<TKey, TValue> node)
        {
            Console.Write(" | ");
            Console.Write("P: " + node.Key);
            if (node.Left != null)
            {
                Console.Write(", L: " + node.Left.Key);
            }

            if (node.Right != null)
            {
                Console.Write(", R: " + node.Right.Key);
            }

            if (node.Left != null)
            {
                PrintConsole(node.Left);
            }

            if (node.Right != null)
            {
                PrintConsole(node.Right);
            }
        }
    }
}