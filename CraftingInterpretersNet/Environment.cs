using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class Environment
{
    private readonly Dictionary<string, object?> _values = new();

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

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (!_values.ContainsKey(name.Lexeme))
        {
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
        _values[name.Lexeme] = value;
    }
    
}