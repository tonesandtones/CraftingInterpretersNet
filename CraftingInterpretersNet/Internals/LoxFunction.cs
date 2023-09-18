using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet.Internals;

public class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function _declaration;
    private readonly Environment _closure;
    private readonly bool _isInitialiser;

    public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitialiser)
    {
        _declaration = declaration;
        _closure = closure;
        _isInitialiser = isInitialiser;
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
            return _isInitialiser ? _closure.GetAt(0, "this") : returnValue.Value;
        }

        return _isInitialiser ? _closure.GetAt(0, "this") : null;
    }

    public int Arity => _declaration.Par.Count;

    public LoxFunction Bind(LoxInstance instance)
    {
        Environment environment = new(_closure);
        environment.Define("this", instance);
        return new LoxFunction(_declaration, environment, _isInitialiser);
    }

    public override string ToString()
    {
        return $"<fn {_declaration.Name.Lexeme}>";
    }
}