using CraftingInterpretersNet.Abstractions;
using FluentAssertions;
using Xunit;

namespace CraftingInterpretersNet.Tests;

public class ParserTests
{
    private readonly CollectingErrorReporter _errorReporter = new();
    
    private Expr? ParsedExpr(string source)
    {
        Scanner s = new(source, _errorReporter);
        Parser p = new(s.ScanTokens());
        return p.Parse();
    }

    [MemberData(nameof(LoxAndExpectedAst))]
    [Theory]
    public void ParserProducesExpectedExpressTree(string lox, string? expectedAst)
    {
        string? result = new AstPrinter().Print(ParsedExpr(lox));
        _errorReporter.HasReceivedError.Should().BeFalse();
        result.Should().Be(expectedAst);
    }

    public static IEnumerable<object?[]> LoxAndExpectedAst()
    {
        // yield return TestCase("", null); //bad test case for now - empty input doesn't parse. Should it?
        // yield return TestCase(" \n ", null); //bad test case for now - empty input doesn't parse. Should it?

        yield return TestCase("true", "True");
        yield return TestCase("(true)", "(group True)");
        yield return TestCase("\"abc\"", "abc");
        yield return TestCase("1+1", "(+ 1 1)");
        yield return TestCase("-1", "(- 1)");
        yield return TestCase("-1-1", "(- (- 1) 1)");

        yield return TestCase("(6 - (2 + 2)) * 2", "(* (group (- 6 (group (+ 2 2)))) 2)");
    }

    private static object?[] TestCase(string lox, string? expectedAst)
    {
        return new object?[] { lox, expectedAst };
    }
}