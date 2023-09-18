using System;
using System.Collections.Generic;

namespace CraftingInterpretersNet.Internals;

public class ClockCallable : ILoxCallable
{
    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public int Arity => 0;

    public override string ToString()
    {
        return "<native fn>";
    }
}