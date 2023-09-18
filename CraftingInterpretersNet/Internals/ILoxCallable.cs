using System.Collections.Generic;

namespace CraftingInterpretersNet.Internals;

public interface ILoxCallable
{
    int Arity { get; }
    object? Call(Interpreter interpreter, List<object> arguments);
}