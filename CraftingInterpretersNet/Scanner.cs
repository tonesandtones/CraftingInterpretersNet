using System;
using System.Collections.Generic;
using CraftingInterpretersNet.Abstractions;
using static CraftingInterpretersNet.Abstractions.TokenType;

namespace CraftingInterpretersNet;

public class Scanner
{
    private readonly IErrorReporter _errorReporter;

    private readonly string _source;
    private readonly List<Token> _tokens = new();

    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private static readonly Dictionary<string, TokenType> keywords = new()
    {
        { "and", AND },
        { "class", CLASS },
        { "else", ELSE },
        { "false", FALSE },
        { "for", FOR },
        { "fun", FUN },
        { "if", IF },
        { "nil", NIL },
        { "or", OR },
        { "print", PRINT },
        { "return", RETURN },
        { "super", SUPER },
        { "this", THIS },
        { "true", TRUE },
        { "var", VAR },
        { "while", WHILE }
    };

    public Scanner(string source) : this(source, Defaults.DefaultErrorReporter)
    {
        _source = source;
    }

    public Scanner(string source, IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
        _source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(EOF, "", null, _line));
        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
      //@formatter:off
      case '(': AddToken(LEFT_PAREN); break;
      case ')': AddToken(RIGHT_PAREN); break;
      case '{': AddToken(LEFT_BRACE); break;
      case '}': AddToken(RIGHT_BRACE); break;
      case ',': AddToken(COMMA); break;
      case '.': AddToken(DOT); break;
      case '-': AddToken(MINUS); break;
      case '+': AddToken(PLUS); break;
      case ';': AddToken(SEMICOLON); break;
      case '*': AddToken(STAR); break;
      case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
      case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
      case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
      case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
      case '?': AddToken(QUESTION); break;
      case ':': AddToken(COLON); break;
            //@formatter:on
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }

                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                _line++;
                break;
            case '"':
                String();
                break;
            default:
                if (IsDigit(c))
                {
                    Number();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    _errorReporter.Error(_line, "Unexpected character _" + c + "_");
                }

                break;
        }
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd())
        {
            _errorReporter.Error(_line, "Unterminated String");
            return;
        }

        //the closing "
        Advance();

        //trim surrounding quotes
        var value = _source[(_start + 1) .. (_current - 1)];
        AddToken(STRING, value);
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        //look for fractional part
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            //consume the '.'
            Advance();
        }

        while (IsDigit(Peek())) Advance();

        AddToken(NUMBER, double.Parse(_source[_start.._current]));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();

        var text = _source[_start .. _current];
        var hasType = keywords.TryGetValue(text, out var type);
        if (!hasType) type = IDENTIFIER;
        AddToken(type);
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }

    private char Advance()
    {
        return _source[_current++];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        var text = _source[_start .. _current];
        _tokens.Add(new Token(type, text, literal, _line));
    }

    private bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }
}