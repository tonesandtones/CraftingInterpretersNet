using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet.Internals;

public class LoxInstance
{
    private LoxClass _klass;
    private readonly Dictionary<string, object?> _fields = new();

    public LoxInstance(LoxClass klass)
    {
        _klass = klass;
    }

    public override string ToString()
    {
        return $"{_klass.Name} instance";
    }


    public object? Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var field)) return field;

        var method = _klass.FindMethod(name.Lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
    }

    public void Set(Token name, object? value)
    {
        _fields[name.Lexeme] = value;
    }
}