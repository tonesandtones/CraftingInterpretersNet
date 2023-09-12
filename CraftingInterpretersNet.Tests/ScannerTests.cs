using CraftingInterpretersNet.Abstractions;
using FluentAssertions;
using Xunit;
using static CraftingInterpretersNet.Abstractions.TokenType;

namespace CraftingInterpretersNet.Tests;

public class ScannerTests
{
    private readonly CollectingErrorReporter _errorReporter = new();

    private List<Token> ScannedTokens(string source)
    {
        Scanner s = new(source, _errorReporter);
        return s.ScanTokens();
    }

    //todo - test cases for deeper comparison involving the token type, lexeme, literal, and line

    [MemberData(nameof(ScannerTokenOrderTestData))]
    [Theory]
    public void ScannerProducesTokensWithTokenTypeInExpectedOrder(string lox, IEnumerable<TokenType> expectedTokenTypes)
    {
        var actualTokenTypes = ScannedTokens(lox).Select(x => x.Type);
        _errorReporter.HasReceivedError.Should().BeFalse();
        actualTokenTypes.Should().Equal(expectedTokenTypes);
    }

    public static IEnumerable<object[]> ScannerTokenOrderTestData()
    {
        yield return TestCase("");
        yield return TestCase("(", LEFT_PAREN);
        yield return TestCase(")", RIGHT_PAREN);
        yield return TestCase("{", LEFT_BRACE);
        yield return TestCase("}", RIGHT_BRACE);
        yield return TestCase(",", COMMA);
        yield return TestCase(".", DOT);
        yield return TestCase("-", MINUS);
        yield return TestCase("+", PLUS);
        yield return TestCase(";", SEMICOLON);
        yield return TestCase("/", SLASH);
        yield return TestCase("*", STAR);
        yield return TestCase("?", QUESTION);
        yield return TestCase(":", COLON);
        yield return TestCase("!", BANG);
        yield return TestCase("!=", BANG_EQUAL);
        yield return TestCase("=", EQUAL);
        yield return TestCase("==", EQUAL_EQUAL);
        yield return TestCase(">", GREATER);
        yield return TestCase(">=", GREATER_EQUAL);
        yield return TestCase("<", LESS);
        yield return TestCase("<=", LESS_EQUAL);
        yield return TestCase("a", IDENTIFIER);
        yield return TestCase("abc", IDENTIFIER);
        yield return TestCase("\"\"", STRING);
        yield return TestCase("\"abc\"", STRING);
        yield return TestCase("1", NUMBER);
        yield return TestCase("0.0", NUMBER);
        yield return TestCase("and", AND);
        yield return TestCase("class", CLASS);
        yield return TestCase("else", ELSE);
        yield return TestCase("false", FALSE);
        yield return TestCase("fun", FUN);
        yield return TestCase("for", FOR);
        yield return TestCase("if", IF);
        yield return TestCase("nil", NIL);
        yield return TestCase("or", OR);
        yield return TestCase("print", PRINT);
        yield return TestCase("return", RETURN);
        yield return TestCase("super", SUPER);
        yield return TestCase("this", THIS);
        yield return TestCase("true", TRUE);
        yield return TestCase("var", VAR);
        yield return TestCase("while", WHILE);

        yield return TestCase("for (var ab = 123 )", FOR, LEFT_PAREN, VAR, IDENTIFIER, EQUAL, NUMBER, RIGHT_PAREN);
        yield return TestCase("-1", MINUS, NUMBER);
        yield return TestCase("while a;\n\nabcd > \"abc\"\n", WHILE, IDENTIFIER, SEMICOLON, IDENTIFIER, GREATER,
            STRING);
    }

    private static object[] TestCase(string lox, params TokenType[] tokenTypes)
    {
        return new object[] { lox, tokenTypes.Append(EOF) };
    }
}