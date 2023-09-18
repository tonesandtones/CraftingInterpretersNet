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
        catch (ParseError)
        {
            return Array.Empty<Stmt>();
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(CLASS)) return ClassDeclaration();
            if (Match(FUN)) return Function("function");
            if (Match(VAR)) return VarDeclaration();
            return Statement();
        }
        catch (ParseError)
        {
            Synchronise();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expect class name.");
        Consume(LEFT_BRACE, "Expect '{' before class body.");

        List<Stmt.Function> methods = new();
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        Consume(RIGHT_BRACE, "Expect '}' after class body.");

        return new Stmt.Class(name, null, methods);
    }

    private Stmt.Function Function(string kind)
    {
        var name = Consume(IDENTIFIER, $"Expect {kind} name.");
        Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");
        List<Token> parameters = new();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255) Error(Peek(), "Can't have more than 255 parameters.");
                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }

        Consume(RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
        var body = Block();
        return new Stmt.Function(name, parameters, body);

    }

    private Stmt VarDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initialiser = null;
        if (Match(EQUAL))
        {
            initialiser = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration");
        return new Stmt.Var(name, initialiser);
    }

    private Stmt Statement()
    {
        if (Match(FOR)) return ForStatement();
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(RETURN)) return ReturnStatement();
        if (Match(WHILE)) return WhileStatement();
        if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt ReturnStatement()
    {
        Token keyword = Previous();
        Expr? value = null;
        if (!Check(SEMICOLON))
        {
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return value");
        return new Stmt.Return(keyword, value);
    }

    private Stmt ForStatement()
    {
        //A for-statement is syntactic sugar over a while loop
        //  var i = 0;  //initialiser
        //  while (i < 10) { //condition
        //    print i; //body
        //    i = i + 1; //increment
        //  }

        Consume(LEFT_PAREN, "Expect '(' after 'for'.");
        Stmt? initialiser;
        if (Match(SEMICOLON)) initialiser = null;
        else if (Match(VAR)) initialiser = VarDeclaration();
        else initialiser = ExpressionStatement();

        Expr? condition = null;
        if (!Check(SEMICOLON)) condition = Expression();

        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(RIGHT_PAREN)) increment = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after for clauses");

        var body = Statement();
        if (increment != null)
        {
            body = new Stmt.Block(
                new List<Stmt> { body, new Stmt.Expression(increment) });
        }

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initialiser != null) body = new Stmt.Block(new List<Stmt> { initialiser, body });

        return body;
    }

    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after condition.");
        var body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        var thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = new();
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(SEMICOLON, "Expect ';' after a value.");
        return new Stmt.Print(value);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Or();
        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();
            if (expr is Expr.Variable ev) return new Expr.Assign(ev.Name, value);
            if (expr is Expr.Get get) return new Expr.Set(get.Obj, get.Name, value);
            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var @operator = Previous();
            var right = And();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Ternary();

        while (Match(AND))
        {
            var @operator = Previous();
            var right = Ternary();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    //ternary        → equality ( "?" expression ":" expression)* ;
    private Expr Ternary()
    {
        var expr = Equality();
        if (Match(QUESTION))
        {
            var left = Expression();
            Consume(COLON, "Expect \':\' in ternary operator");
            var right = Expression();
            expr = new Expr.Conditional(expr, left, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();
        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var @operator = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();
        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var @operator = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();
        while (Match(MINUS, PLUS))
        {
            var @operator = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();
        while (Match(SLASH, STAR))
        {
            var @operator = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            var @operator = Previous();
            var right = Unary();
            return new Expr.Unary(@operator, right);
        }

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();
        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            } else if (Match(DOT))
            {
                var name = Consume(IDENTIFIER, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else break;
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            }
            while (Match(COMMA));
        }

        var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");
        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING)) return new Expr.Literal(Previous().Literal);

        if (Match(THIS)) return new Expr.This(Previous());

        if (Match(IDENTIFIER)) return new Expr.Variable(Previous());

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
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

    private ParseError Error(Token token, string message)
    {
        _errorReporter.Error(token, message);
        return new ParseError();
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

    public class ParseError : Exception
    {
    }
}