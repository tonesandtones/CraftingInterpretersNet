﻿using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class Resolver : BaseVisitor<object, object>
{
    private enum FunctionType
    {
        NONE,
        FUNCTION,
        INITIALISER,
        METHOD
    }

    private enum ClassType
    {
        NONE,
        CLASS,
        SUBCLASS,
    }

    private readonly IErrorReporter _errorReporter;
    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, bool>> _scopes = new();

    private FunctionType _currentFunction = FunctionType.NONE;
    private ClassType _currentClass = ClassType.NONE;

    public Resolver(Interpreter interpreter) : this(interpreter, Defaults.DefaultErrorReporter)
    {
    }

    public Resolver(Interpreter interpreter, IErrorReporter errorReporter)
    {
        _interpreter = interpreter;
        _errorReporter = errorReporter;
    }

    public override object? VisitClassStmt(Stmt.Class stmt)
    {
        var enclosingClass = _currentClass;
        _currentClass = ClassType.CLASS;

        Declare(stmt.Name);
        Define(stmt.Name);

        if (stmt.Superclass != null && stmt.Name.Lexeme.Equals(stmt.Superclass.Name.Lexeme))
        {
            _errorReporter.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
        }

        if (stmt.Superclass != null)
        {
            _currentClass = ClassType.SUBCLASS;
            Resolve(stmt.Superclass);
        }

        if (stmt.Superclass != null)
        {
            BeginScope();
            _scopes.Peek()["super"] = true;
        }

        BeginScope();
        _scopes.Peek()["this"] = true;

        foreach (var method in stmt.Methods)
        {
            var declaration = method.Name.Lexeme.Equals("init")
                ? FunctionType.INITIALISER
                : FunctionType.METHOD;
            ResolveFunction(method, declaration);
        }

        EndScope();

        if (stmt.Superclass != null) EndScope();

        _currentClass = enclosingClass;
        return null;
    }

    public override object? VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Obj);
        return null;
    }

    public override object? VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Obj);
        return null;
    }

    public override object? VisitSuperExpr(Expr.Super expr)
    {
        if (_currentClass == ClassType.NONE)
        {
            _errorReporter.Error(expr.Keyword, "Can't use 'super' outside of a class.");
        } else if (_currentClass != ClassType.SUBCLASS)
        {
            _errorReporter.Error(expr.Keyword, "Can't use 'super' in a class with no superclass.");
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public override object? VisitThisExpr(Expr.This expr)
    {
        if (_currentClass == ClassType.NONE)
        {
            _errorReporter.Error(expr.Keyword, "Can't use 'this' outside of a class.");
            return null;
        }

        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public override object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public override object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public override object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
            ;
        }

        Define(stmt.Name);
        return null;
    }

    public override object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public override object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public override object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);
        foreach (var arg in expr.Arguments)
        {
            Resolve(arg);
        }

        return null;
    }

    public override object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public override object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public override object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public override object? VisitConditionalExpr(Expr.Conditional expr)
    {
        Resolve(expr.Condition);
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public override object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public override object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_scopes.Count != 0)
        {
            var scope = _scopes.Peek();
            if (scope.TryGetValue(expr.Name.Lexeme, out var result) && result == false)
            {
                _errorReporter.Error(expr.Name, "Can't read local variable in its own initialiser.");
            }
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public override object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public override object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
        return null;
    }

    public override object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public override object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (_currentFunction == FunctionType.NONE)
        {
            _errorReporter.Error(stmt.Keyword, "Can't return from top-level code.");
        }

        if (stmt.Value == null) return null;

        if (_currentFunction == FunctionType.INITIALISER)
        {
            _errorReporter.Error(stmt.Keyword, "Can't return from an initialiser.");
        }

        Resolve(stmt.Value);

        return null;
    }

    public override object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;

        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            _errorReporter.Error(name, "Already a variable with this name in this scope.");
        }

        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes.Peek()[name.Lexeme] = true;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var scopesList = _scopes.ToArray(); //unlike java, .Net Stack doesn't have random access members

        //slight difference from the book. Traversal order is opposite between Java's Stack.get(i) and .Net Stack.ToArray()
        for (var i = 0; i < scopesList.Length; i++)
        {
            if (scopesList[i].ContainsKey(name.Lexeme))
            {
                _interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    public void Resolve(IEnumerable<Stmt> statements)
    {
        foreach (var stmt in statements)
        {
            Resolve(stmt);
        }
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunctionType = _currentFunction;
        _currentFunction = type;

        BeginScope();
        foreach (var param in function.Par)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);
        EndScope();

        _currentFunction = enclosingFunctionType;
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        _scopes.TryPop(out _);
    }
}