﻿namespace DataStructures.structures.stack
{
    public class Node<T>
    {
        public Node<T> Next { set; get; }
        public T Val { set; get; }
        
        public Node(T val) {
            Val = val;
        }
    }
}