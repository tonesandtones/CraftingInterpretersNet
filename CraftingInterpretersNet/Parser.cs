using System;
using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;
using static CraftingInterpretersNet.Abstractions.TokenType;

namespace CraftingInterpretersNet;

public class Parser
{
    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(IEnumerable<Token> tokens)
    {
        this.tokens = new List<Token>(tokens);
    }

    public Expr? Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseErrorException e)
        {
            return null;
        }
    }

    private Expr Expression()
    {
        return Equality();
    }

    private Expr Equality()
    {
        Expr expr = Comparison();
        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            Token @operator = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();
        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            Token @operator = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        Expr expr = Factor();
        while (Match(MINUS, PLUS))
        {
            Token @operator = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }
    
    private Expr Factor()
    {
        Expr expr = Unary();
        while (Match(SLASH, STAR))
        {
            Token @operator = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            Token @operator = Previous();
            Expr right = Unary();
            return new Expr.Unary(@operator, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING)) return new Expr.Literal(Previous().Literal);

        if (Match(LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression");
            return new Expr.Grouping(expr);
        }
        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private void Synchronise()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON) return;

            //
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    private ParseErrorException Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseErrorException();
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }
    
    public class ParseErrorException : Exception
    {
    }
}