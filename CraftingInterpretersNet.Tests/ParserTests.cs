using CraftingInterpretersNet.Abstractions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class ParserTests
{
    private readonly CollectingErrorReporter _errorReporter = new();
    private readonly ITestOutputHelper _testOutput;

    public ParserTests(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
    }

    private Expr? ParsedExpr(string source)
    {
        Scanner s = new(source, _errorReporter);
        Parser p = new(s.ScanTokens(), _errorReporter);
        return p.Parse();
    }

    [MemberData(nameof(LoxAndExpectedAst))]
    [Theory]
    public void ParserProducesExpectedExpressTree(string lox, string? expectedAst)
    {
        try
        {
            string? result = new AstPrinter().Print(ParsedExpr(lox));
            _errorReporter.HasReceivedError.Should().BeFalse();
            result.Should().Be(expectedAst);
        }
        catch (Exception e)
        {
            PrintErrorReporterContents();
            throw;
        }
    }

    private void PrintErrorReporterContents()
    {
        if (!_errorReporter.HasReceivedError)
        {
            _testOutput.WriteLine("Error reported had no errors");
        }
        else
        {
            _testOutput.WriteLine("Received an error. Parser error reporter had these errors:");
            foreach (var errorEvent in _errorReporter.ReceivedErrors)
            {
                _testOutput.WriteLine($"{errorEvent.Message} : {errorEvent.Where}");
            }
        }
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
        yield return TestCase("\"a\" == true", "(== a True)");

        yield return TestCase("(6 - (2 + 2)) * 2", "(* (group (- 6 (group (+ 2 2)))) 2)");

        yield return TestCase("1 ? 1 : 1", "(?: 1 1 1)");
        yield return TestCase("\"a\" == 1 ? 1 : 1", "(?: (== a 1) 1 1)");
        yield return TestCase("\"b\" > 1 ? 123 ? 2 : 3 : 4", "(?: (> b 1) (?: 123 2 3) 4)");
        yield return TestCase("\"c\" > 1 ? 123 : 1 ? 3 : 4", "(?: (> c 1) 123 (?: 1 3 4))"); 
        yield return TestCase("\"cc\" > 1 ? (123 ? 1 : 3) : 4", "(?: (> cc 1) (group (?: 123 1 3)) 4)");
        yield return TestCase("(\"d\" > 1 ? 123 : 1) ? 3 : 4", "(?: (group (?: (> d 1) 123 1)) 3 4)");
    }

    private static object?[] TestCase(string lox, string? expectedAst)
    {
        return new object?[] { lox, expectedAst };
    }
}