using System.Collections.Generic;

namespace CraftingInterpretersNet.Internals;

public class LoxClass : ILoxCallable
{
    public string Name { get; private set; }

    private readonly Dictionary<string, LoxFunction> _methods;

    public LoxClass(string name, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        _methods = methods;
    }

    public override string ToString()
    {
        return Name;
    }

    public int Arity => FindMethod("init")?.Arity ?? 0;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new LoxInstance(this);
        var initialiser = FindMethod("init");
        initialiser?.Bind(instance).Call(interpreter, arguments);
        return instance;
    }

    public LoxFunction? FindMethod(string name)
    {
        return _methods.TryGetValue(name, out var method) ? method : null;
    }
}