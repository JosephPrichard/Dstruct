﻿using System;
using System.Runtime.Serialization;

namespace DStruct.Heap;

public class EmptyHeapException : Exception
{
    public EmptyHeapException()
    {
    }

    public EmptyHeapException(string message) : base(message)
    {
    }

    public EmptyHeapException(string message, Exception inner) : base(message, inner)
    {
    }

    protected EmptyHeapException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}