using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function _declaration;

    public LoxFunction(Stmt.Function declaration)
    {
        _declaration = declaration;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        Environment environment = new Environment(interpreter.Globals);
        for (var i = 0; i < _declaration.Par.Count; i++)
        {
            var param = _declaration.Par[i];
            environment.Define(param.Lexeme, arguments[i]);
        }

        interpreter.ExecuteBlock(_declaration.Body, environment);
        return null;
    }

    public int Arity => _declaration.Par.Count;

    public override string ToString()
    {
        return $"<fn {_declaration.Name.Lexeme}>";
    }
}