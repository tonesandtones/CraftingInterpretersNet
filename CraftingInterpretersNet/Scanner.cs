using System.Collections.Generic;
using static CraftingInterpretersNet.TokenType;

namespace CraftingInterpretersNet;

public class Scanner
{
  private readonly string source;
  private readonly List<Token> tokens = new();

  private int start = 0;
  private int current = 0;
  private int line = 1;

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

  public Scanner(string source)
  {
    this.source = source;
  }

  public List<Token> ScanTokens()
  {
    while (!IsAtEnd())
    {
      start = current;
      ScanToken();
    }

    tokens.Add(new Token(EOF, "", null, line));
    return tokens;
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
        line++;
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
          Program.Error(line, "Unexpected character _" + c + "_");
        }

        break;
    }
  }

  private void String()
  {
    while (Peek() != '"' && !IsAtEnd())
    {
      if (Peek() == '\n') line++;
      Advance();
    }

    if (IsAtEnd())
    {
      Program.Error(line, "Unterminated String");
      return;
    }

    //the closing "
    Advance();

    //trim surrounding quotes
    var value = source.Substring(start + 1, current - 1 - (start + 1));
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

    AddToken(NUMBER, double.Parse(source.Substring(start, current-start)));
  }

  private void Identifier()
  {
    while (IsAlphaNumeric(Peek())) Advance();

    var text = source.Substring(start, current-start);
    var hasType = keywords.TryGetValue(text, out var type);
    if (!hasType) type = IDENTIFIER;
    AddToken(type);
  }

  private bool IsAtEnd()
  {
    return current >= source.Length;
  }

  private char Advance()
  {
    return source[current++];
  }

  private bool Match(char expected)
  {
    if (IsAtEnd()) return false;
    if (source[current] != expected) return false;

    current++;
    return true;
  }

  private char Peek()
  {
    if (IsAtEnd()) return '\0';
    return source[current];
  }

  private char PeekNext()
  {
    if (current + 1 >= source.Length) return '\0';
    return source[current + 1];
  }

  private void AddToken(TokenType type, object? literal = null)
  {
    var text = source.Substring(start, current-start);
    tokens.Add(new Token(type, text, literal, line));
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