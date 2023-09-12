﻿using System.Collections.Generic;
using System.Globalization;
using CraftingInterpretersNet.Abstractions;
using static CraftingInterpretersNet.Abstractions.TokenType;

namespace CraftingInterpretersNet;

public class Interpreter : BaseVisitor<object, object?>
{
    private readonly IRuntimeErrorReporter _errorReporter;
    private readonly IOutputSink _sink;

    public Interpreter(): this(Defaults.DefaultRuntimeErrorReporter, Defaults.DefaultOutputSink)
    {
    }

    public Interpreter(IRuntimeErrorReporter errorReporter, IOutputSink sink)
    {
        _errorReporter = errorReporter;
        _sink = sink;
    }

    public void Interpret(IEnumerable<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
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

    public override object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public override object? VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.Expr);
        _sink.Print(Stringify(value));
        return null;
    }

    public override object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public override object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public override object VisitUnaryExpr(Expr.Unary expr)
    {
        object right = Evaluate(expr.Right);
        switch (expr.Oper.Type)
        {
            case MINUS:
                CheckNumberOperand(expr.Oper, right);
                return -(double)right;
            case BANG:
                return !IsTruthy(right);
        }

        //unreachable. The only unary operators we have are MINUS and BANG
        return null;
    }


    public override object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Oper.Type)
        {
            case MINUS:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left - (double)right;
            case SLASH:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left * (double)right;
            case PLUS:
                CheckAdditionOperands(expr.Oper, left, right);
                if (left is double ld && right is double rd) return ld + rd;
                if (left is string ls && right is string rs) return ls + rs;
                break;
            case GREATER:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left <= (double)right;
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
        }

        //unreachable.
        return null;
    }

    public override object VisitConditionalExpr(Expr.Conditional expr)
    {
        object condition = Evaluate(expr.Condition);

        return IsTruthy(condition) ? Evaluate(expr.Left) : Evaluate(expr.Right);
    }

    private object Evaluate(Expr expr)
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

    private void CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token @operator, object left, object right)
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