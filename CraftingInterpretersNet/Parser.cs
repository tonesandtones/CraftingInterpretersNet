using System;
using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;
using static CraftingInterpretersNet.Abstractions.TokenType;

namespace CraftingInterpretersNet;

public class Parser
{
    private readonly IErrorReporter _errorReporter;
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(IEnumerable<Token> tokens) : this(tokens, Defaults.DefaultErrorReporter)
    {
    }

    public Parser(IEnumerable<Token> tokens, IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        _tokens = new List<Token>(tokens);
    }

    public IEnumerable<Stmt> Parse()
    {
        List<Stmt> statements = new();
        try
        {
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }
        }
        catch (ParseErrorException)
        {
            return Array.Empty<Stmt>();
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(VAR)) return VarDeclaration();
            return Statement();
        }
        catch (ParseErrorException)
        {
            Synchronise();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initialiser = null;
        if (Match(EQUAL_EQUAL))
        {
            initialiser = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration");
        return new Stmt.Var(name, initialiser);
    }

    private Stmt Statement()
    {
        if (Match(PRINT)) return PrintStatement();

        return ExpressionStatement();
    }

    private Stmt PrintStatement()
    {
        Expr value = Expression();
        Consume(SEMICOLON, "Expect ';' after a value.");
        return new Stmt.Print(value);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr Expression()
    {
        return
            Ternary(); //Implemented as part of ch6 challenges https://craftinginterpreters.com/parsing-expressions.html#challenges
        // return Equality(); //replace Ternary() with Expression() to go back to what the book does.
    }

    //ternary        → equality ( "?" expression ":" expression)* ;
    private Expr Ternary()
    {
        Expr expr = Equality();
        if (Match(QUESTION))
        {
            Token leftOperator = Previous();
            Expr left = Expression();
            Token rightOperator = Consume(COLON, "Expect \':\' in ternary operator");
            Expr right = Expression();
            expr = new Expr.Conditional(expr, leftOperator, left, rightOperator, right);
        }

        return expr;
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

        if (Match(IDENTIFIER)) return new Expr.Variable(Previous());

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
        _errorReporter.Error(token, message);
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
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    public class ParseErrorException : Exception
    {
    }
}