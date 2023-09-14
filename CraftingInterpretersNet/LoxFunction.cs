using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function _declaration;
    private readonly Environment _closure;

    public LoxFunction(Stmt.Function declaration, Environment closure)
    {
        _declaration = declaration;
        _closure = closure;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        Environment environment = new(_closure);
        for (var i = 0; i < _declaration.Par.Count; i++)
        {
            var param = _declaration.Par[i];
            environment.Define(param.Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }
        return null;
    }

    public int Arity => _declaration.Par.Count;

    public override string ToString()
    {
        return $"<fn {_declaration.Name.Lexeme}>";
    }
}