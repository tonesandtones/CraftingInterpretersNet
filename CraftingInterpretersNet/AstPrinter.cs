using System;
using System.Linq;
using CraftingInterpretersNet.Abstractions;

namespace CraftingInterpretersNet;

public class AstPrinter : BaseExprVisitor<string>
{
    public static void Sample()
    {
        Expr expression = new Expr.Binary(
            new Expr.Unary(
                new Token(TokenType.MINUS, "-", null, 1),
                new Expr.Literal(123)),
            new Token(TokenType.STAR, "*", null, 1),
            new Expr.Grouping(
                new Expr.Literal(45.67)));
        
        Console.WriteLine(new AstPrinter().Print(expression));
    }
    
    public string? Print(Expr? expr)
    {
        return expr?.Accept(this);
    }

    private string Parenthesise(string name, params Expr[] exprs)
    {
        return $"({name} {string.Join(' ', exprs.Select(x => x.Accept(this)))})";
    }

    public override string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesise(expr.Oper.Lexeme, expr.Left, expr.Right);
    }

    public override string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesise("group", expr.Expression);
    }

    public override string VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value == null ? "nil" : expr.Value.ToString() ?? "";
    }

    public override string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesise(expr.Oper.Lexeme, expr.Right);
    }
}