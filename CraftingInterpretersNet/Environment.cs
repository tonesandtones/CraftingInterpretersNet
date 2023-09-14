using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class Environment
{
    private readonly Dictionary<string, object?> _values = new();
    private readonly Environment? _enclosing;

    public Environment() : this(null)
    {
    }

    public Environment(Environment? enclosing)
    {
        _enclosing = enclosing;
    }

    public void Define(string name, object? value)
    {
        _values[name] = value;
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var result))
        {
            return result;
        }

        //We can't just do _enclosing?.Get(name) because we need to throw if there is no enclosing environment.
        if (_enclosing != null) return _enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (_enclosing != null)
        {
            _enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)?._values[name];
    }

    private Environment? Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
        {
            environment = environment?._enclosing;
        }

        return environment;
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)!._values[name.Lexeme] = value;
    }
}