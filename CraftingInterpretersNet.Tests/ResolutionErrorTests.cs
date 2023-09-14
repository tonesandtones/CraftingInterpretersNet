using CraftingInterpretersNet.Abstractions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CraftingInterpretersNet.Tests;

public class ResolutionErrorTests
{
    private readonly CollectingErrorReporter _errorReporter = new();
    private readonly CollectingRuntimeErrorReporter _runtimeReporter = new();
    private readonly CollectingOutputSink _sink = new();

    private readonly ITestOutputHelper _outputHelper;

    public ResolutionErrorTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [MemberData(nameof(RuntimeErrorTestCases))]
    [Theory]
    public void RuntimeErrorTests(string lox, params string[] expectedErrors)
    {
        Interpret(lox);

        _errorReporter.HasReceivedError.Should().BeTrue();
        _errorReporter.ReceivedErrors.Should().HaveCount(expectedErrors.Length)
            .And.Subject.Select(x => x.Message).ToList()
            .Should().BeEquivalentTo(expectedErrors);
    }

    public static IEnumerable<object[]> RuntimeErrorTestCases()
    {
        yield return TestCase( //not a valid test case, you can't refer to a variable in its own initialiser
            """
            var a = 1;
            {
              print 1;
              var a = a + 2;
              print a;
            }
            print a;
            """,
            "Can't read local variable in its own initialiser.");
    }


    private void Interpret(string lox)
    {
        Scanner s = new(lox, _errorReporter);
        var tokens = s.ScanTokens();
        Parser p = new(tokens, _errorReporter);
        var parsedExpr = p.Parse().ToList();

        FailIfErrors(parsedExpr, "parse");

        Interpreter i = new(_runtimeReporter, new MultiSink(_sink, new TestOutputHelperSink(_outputHelper)));
        Resolver resolver = new(i, _errorReporter);
        resolver.Resolve(parsedExpr);
    }

    private void FailIfErrors(List<Stmt> parsedExpr, string stage)
    {
        if (parsedExpr is { Count: 0 } || _errorReporter.HasReceivedError)
        {
            _outputHelper.WriteLine($"Did not receive output from {stage}.");
            if (!_errorReporter.HasReceivedError)
            {
                _outputHelper.WriteLine("Error reporter did not receive any errors.");
            }
            else
            {
                _outputHelper.WriteLine("Error reporter had these errors:");
                foreach (var errorEvent in _errorReporter.ReceivedErrors)
                {
                    _outputHelper.WriteLine($"{errorEvent.Message} : {errorEvent.Where} : line {errorEvent.Line}");
                }
            }

            Assert.Fail($"Did not receive any output from {stage}");
        }
    }

    private static object[] TestCase(string lox, params string[] expectedErrors)
    {
        return new object[] { lox, expectedErrors };
    }
}