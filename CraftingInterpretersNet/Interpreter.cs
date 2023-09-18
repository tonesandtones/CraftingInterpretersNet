using System.Collections.Generic;
using System.Globalization;
using CraftingInterpretersNet.Abstractions;
using CraftingInterpretersNet.Internals;
using static CraftingInterpretersNet.Abstractions.TokenType;

namespace CraftingInterpretersNet;

public class Interpreter : BaseVisitor<object, object?>
{
    private readonly IRuntimeErrorReporter _errorReporter;
    private readonly IOutputSink _sink;
    private readonly Environment _globals = new();
    private Environment _environment;
    private readonly Dictionary<Expr, int> _locals = new();

    public Interpreter() : this(Defaults.DefaultRuntimeErrorReporter, Defaults.DefaultOutputSink)
    {
    }

    public Interpreter(IRuntimeErrorReporter errorReporter, IOutputSink sink)
    {
        _errorReporter = errorReporter;
        _sink = sink;
        _environment = _globals;
        
        _globals.Define("clock", new ClockCallable());
    }

    public void Interpret(IEnumerable<Stmt?> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                if (statement != null) Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            _errorReporter.Error(e);
        }
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    internal void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }

    public override object? VisitClassStmt(Stmt.Class stmt)
    {
        _environment.Define(stmt.Name.Lexeme, null);

        Dictionary<string, LoxFunction> methods = new();
        foreach (var method in stmt.Methods)
        {
            LoxFunction function = new(method, _environment);
            methods[method.Name.Lexeme] = function;
        }
        
        LoxClass klass = new LoxClass(stmt.Name.Lexeme, methods);
        _environment.Assign(stmt.Name, klass);
        return null;
    }

    public override object? VisitGetExpr(Expr.Get expr)
    {
        var obj = Evaluate(expr.Obj);
        if (obj is LoxInstance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeError(expr.Name, "Only instances can have properties.");
    }

    public override object? VisitSetExpr(Expr.Set expr)
    {
        var obj = Evaluate(expr.Obj);
        if (obj is not LoxInstance instance) throw new RuntimeError(expr.Name, "Only instances have fields");
        var value = Evaluate(expr.Value);
        instance.Set(expr.Name, value);
        return value;
    }

    public override object? VisitThisExpr(Expr.This expr)
    {
        return LookupVariable(expr.Keyword, expr);
    }

    public override object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    internal void ExecuteBlock(List<Stmt> statements, Environment environment) 
    {
        var previous = _environment;
        try
        {
            _environment = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public override object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public override object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public override object? VisitVariableExpr(Expr.Variable expr)
    {
        return LookupVariable(expr.Name, expr);
    }

    private object? LookupVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
        {
            return _environment.GetAt(distance, name.Lexeme);
        }

        return _globals.Get(name);
    }

    public override object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        if (_locals.TryGetValue(expr, out var distance))
        {
            _environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            _globals.Assign(expr.Name, value);
        }
        
        return value;
    }

    public override object? VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);
        List<object> arguments = new();
        foreach (var argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument)!);
        }

        if (callee is not ILoxCallable lc)
        {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        }

        if (arguments.Count != lc.Arity)
        {
            throw new RuntimeError(expr.Paren, $"Expected {lc.Arity} arguments but got {arguments.Count}.");
        }
        
        return lc.Call(this, arguments);
    }

    public override object? VisitFunctionStmt(Stmt.Function stmt)
    {
        LoxFunction function = new(stmt, _environment);
        _environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public override object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public override object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        _sink.Print(Stringify(value));
        return null;
    }

    public override object VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.Value != null) value = Evaluate(stmt.Value);
        
        throw new Return(value);
    }

    public override object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public override object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public override object? VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (expr.Oper.Type)
        {
            case MINUS:
                CheckNumberOperand(expr.Oper, right);
                return -(double)right!;
            case BANG:
                return !IsTruthy(right);
        }

        //unreachable. The only unary operators we have are MINUS and BANG
        return null;
    }


    public override object? VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Oper.Type)
        {
            case MINUS:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! - (double)right!;
            case SLASH:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! / (double)right!;
            case STAR:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! * (double)right!;
            case PLUS:
                CheckAdditionOperands(expr.Oper, left, right);
                if (left is double ld && right is double rd) return ld + rd;
                if (left is string ls && right is string rs) return ls + rs;
                break;
            case GREATER:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! > (double)right!;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! >= (double)right!;
            case LESS:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! < (double)right!;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! <= (double)right!;
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
        }

        //unreachable.
        return null;
    }

    public override object? VisitConditionalExpr(Expr.Conditional expr)
    {
        var condition = Evaluate(expr.Condition);

        return IsTruthy(condition) ? Evaluate(expr.Left) : Evaluate(expr.Right);
    }

    public override object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return null;
    }

    public override object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);
        if (expr.Oper.Type == OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private static bool IsTruthy(object? obj)
    {
        return obj switch
        {
            null => false,
            bool b => b,
            _ => true
        };
    }

    private bool IsEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null) return false;
        return left.Equals(right);
    }

    private void CheckNumberOperand(Token @operator, object? operand)
    {
        if (operand is double) return;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token @operator, object? left, object? right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(@operator, "Operands must be numbers.");
    }

    private void CheckAdditionOperands(Token @operator, object? left, object? right)
    {
        if (left is double && right is double ||
            left is string && right is string) return;

        //need to clean up this message.
        //Shouldn't expose C# "Double" or "String" name. Instead should return "number" and "string"
        throw new RuntimeError(@operator, $"Both operands must be the same type and both strings or numbers. " +
                                          $"Left is {left?.GetType().Name ?? "nil"}, right is {right?.GetType().Name ?? "nil"}");
    }

    private string Stringify(object? obj)
    {
        return obj switch
        {
            null => "nil",
            bool b => b ? "true" : "false",
            double d => d.ToString(CultureInfo.InvariantCulture),
            _ => $"{obj}"
        };
    }
}