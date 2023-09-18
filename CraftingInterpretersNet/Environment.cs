using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class Environment
{
    private readonly Dictionary<string, object?> _values = new();
    public Environment? Enclosing { get; }

    public Environment() : this(null)
    {
    }

    public Environment(Environment? enclosing)
    {
        Enclosing = enclosing;
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
        if (Enclosing != null) return Enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
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
            environment = environment?.Enclosing;
        }

        return environment;
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)!._values[name.Lexeme] = value;
    }
}